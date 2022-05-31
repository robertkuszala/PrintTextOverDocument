using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintTextOverDocument
{
    public class Print
    {
        public static void PrintDocument(string path, string printerName, bool onlyFirstPage, string textHeader = "", string textFooter = "")
        {
            var pages = onlyFirstPage ? new List<int> { 1 } : new List<int> { };

            var pageDimensions = iTextSharpWrapper.GetPageDimensions(path, 1);
            var scaleTo = 1;

            bool isLandscape = pageDimensions.Width > pageDimensions.Height;
            bool isA3 = pageDimensions.Width > 1000 || pageDimensions.Height > 1000;

            var texts = new List<iTextSharpWrapper.Text>();
            if (!string.IsNullOrEmpty(textHeader))
            {
                texts.Add(
                    new iTextSharpWrapper.Text
                    {
                        Value = textHeader,
                        Position = new Point(
                        isA3 || isLandscape ? 50 : 46,
                        isA3 || isLandscape ? 28 : 28),
                        FontSize = isA3 || isLandscape ? 12 : 8,
                        PagesToPrintOn = new[] { 1 }
                    });
            }
            if (!string.IsNullOrEmpty(textFooter))
            {
                texts.Add(
                    new iTextSharpWrapper.Text
                    {
                        Value = textFooter,
                        Position = new Point(
                        isA3 || isLandscape ? 50 : 46,
                        isA3 || isLandscape ? (int)pageDimensions.Height - 20 : (int)pageDimensions.Height - 21),
                        FontSize = isA3 || isLandscape ? 12 : 8,
                        PagesToPrintOn = new[] { 1 }
                    });
            }

            // add Text-over
            using (MemoryStream ms = iTextSharpWrapper.WriteToPDF(path, scaleTo, pages, texts))
            {
                if (!RawPrinterHelper.SendStreamToPrinter(ms, printerName, Path.GetFileName(path)))
                    throw new Exception("RawPrinterHelper failed to send stream to the specified printer.");
            }
        }

    }
}
