namespace PrintTextOverDocument
{
    public class DocumentSize
    {
        public enum Sizes
        {
            Original = 0,
            A4 = 1,
            A3 = 2
        }

        public string Name { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int ID { get; set; }

        public static DocumentSize GetDocumentSize(Sizes size)
        {
            switch (size)
            {
                case Sizes.Original:
                    return new DocumentSize { ID = 0, Name = "Original", Width = 0, Height = 0 };
                case Sizes.A4:
                    return new DocumentSize { ID = 1, Name = "A4", Width = 595.276f, Height = 841.89f };
                case Sizes.A3:
                    return new DocumentSize { ID = 2, Name = "A3", Width = 1190.55f, Height = 841.89f };
            }
            throw new ArgumentException($"Size [{Enum.GetName(typeof(DocumentSize.Sizes), size)}] Not Supported.");
        }
    }
}
