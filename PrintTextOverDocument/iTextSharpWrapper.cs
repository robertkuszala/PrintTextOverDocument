using iTextSharp.text;
using iTextSharp.text.pdf;

namespace PrintTextOverDocument
{
    public class iTextSharpWrapper
    {
        public class Text
        {
            public string Value { get; set; }
            /// <summary>
            /// 1st quadrant ([0,0] = lower-left corner, [x,y] = upper-right corner)
            /// </summary>
            public System.Drawing.Point Position { get; set; }
            public float FontSize { get; set; }

            /// <summary>
            /// Page numbers start with 1
            /// </summary>
            public IEnumerable<int> PagesToPrintOn { get; set; }

            /// <summary>
            /// 0 = LEFT, 1 = CENTER, 2 = RIGHT
            /// </summary>
            public int Alignment { get; set; } = 0;

            public bool ShouldBePrintedOnPage(int pageNumber) =>
                (PagesToPrintOn?.Any() ?? false) ? true : PagesToPrintOn.Contains(pageNumber);
        }

        public static Rectangle GetPageDimensions(string filepath, int page)
        {
            PdfReader reader = new PdfReader(filepath);
            return reader.GetPageSizeWithRotation(page);
        }


        /// <summary>
        /// Open existing PDF and write lines of text on to it.
        /// </summary>
        /// <param name="filepath">Full file path to PDF.</param>
        /// <param name="textAbove">Text to write over the document.</param>
        /// <param name="posX">X coordinate of the Text</param>
        /// <param name="posY">Y coordinate of the Text</param>
        /// <param name="fontSize">Font size in points.</param>
        /// <returns>Closed Memory Stream which contains the modified PDF document.</returns>       
        public static MemoryStream WriteToPDF(string filepath, int scaleTo, IEnumerable<int> pages, IEnumerable<Text> texts)
        {
            MemoryStream ms = new MemoryStream();

            // Target Size
            Rectangle outputDocumentSize = new Rectangle(0, 0);

            //Bind a reader to the bytes that we created above
            using (var reader = new PdfReader(filepath))
            {
                //Store our page count
                var pageCount = reader.NumberOfPages;

                //Bind a stamper to our reader
                using (var stamper = new PdfStamper(reader, ms))
                {
                    stamper.Writer.CloseStream = false;

                    //Setup a font to use
                    var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                    //Loop through each page
                    for (var i = 1; i <= pageCount; i++)
                    {
                        if (pages.Any() && !pages.Contains(i))
                            continue; // skip page if not specified (empty collection means "all pages")

                        outputDocumentSize = reader.GetPageSizeWithRotation(i);

                        //Get the raw PDF stream "on top" of the existing content
                        var cb = stamper.GetOverContent(i);

                        // Stamp all texts
                        foreach (var text in texts)
                        {
                            if (!text.ShouldBePrintedOnPage(i))
                                continue;

                            cb.BeginText();
                            cb.SetFontAndSize(baseFont, text.FontSize);

                            var lineNumber = 0;
                            var lineHeight = text.FontSize + 2; // px
                            foreach (var line in text.Value.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                cb.ShowTextAligned(text.Alignment, line, text.Position.X, outputDocumentSize.Height - text.Position.Y - (lineNumber * lineHeight), 0);
                                lineNumber++;
                            }

                            cb.EndText();
                        }
                    }
                }
            }

            // reprint needed
            var reprintNeeded = false;
            if (scaleTo != (int)DocumentSize.Sizes.Original)
            {
                reprintNeeded = true;
                var targetSize = DocumentSize.GetDocumentSize((DocumentSize.Sizes)scaleTo);
                var isLandscape = outputDocumentSize.Width > outputDocumentSize.Height;
                outputDocumentSize = new Rectangle(0, 0, targetSize.Width, targetSize.Height);
                if (isLandscape) outputDocumentSize = outputDocumentSize.Rotate();
            }

            if (pages?.Any() ?? false)
            {
                reprintNeeded = true;
            }

            if (reprintNeeded)
            {
                ms = PrintSelectedPages(ms, pages, outputDocumentSize);
            }

            return ms;
        }

        public static MemoryStream PrintSelectedPages(MemoryStream ms, IEnumerable<int> pages, Rectangle targetSize)
        {
            PdfReader resizeReader = new PdfReader(ms.ToArray());
            ms = new MemoryStream();

            //float newWidth = Math.Min(targetSize.Width, targetSize.Height);
            //float newHeight = Math.Max(targetSize.Width, targetSize.Height);
            //Rectangle newRect = new Rectangle(0, 0, newWidth, newHeight);
            Document document = new Document(targetSize);
            Document.Compress = true;

            PdfWriter resizeWriter = PdfWriter.GetInstance(document, ms);
            resizeWriter.CloseStream = false;
            document.Open();

            PdfContentByte cb = resizeWriter.DirectContent;

            for (int pageNumber = 1; pageNumber <= resizeReader.NumberOfPages; pageNumber++)
            {
                if (pages.Any() && !pages.Contains(pageNumber))
                    continue; // skip page if not specified (empty collection means "all pages")

                PdfImportedPage page = resizeWriter.GetImportedPage(resizeReader, pageNumber);
                document.SetPageSize(targetSize);
                document.NewPage();

                var widthFactor = document.PageSize.Width / page.Width;
                var heightFactor = document.PageSize.Height / page.Height;
                var factor = Math.Min(widthFactor, heightFactor);

                var offsetX = (document.PageSize.Width - (page.Width * factor)) / 2;
                var offsetY = (document.PageSize.Height - (page.Height * factor)) / 2;

                cb.AddTemplate(page, factor, 0, 0, factor, offsetX, offsetY);
            }

            document.Close();

            return ms;
        }

        public static void PrintPDF(MemoryStream ms, string printName)
        {

        }

    }
}
