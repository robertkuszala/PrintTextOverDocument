// See https://aka.ms/new-console-template for more information
Console.WriteLine("==[ Print Text Over PDF Document ]==");

Console.WriteLine("Enter PDF file path:");
var filePath = Console.ReadLine().Replace("\"","");

var defaultPrinter = PrintTextOverDocument.RawPrinterHelper.GetDefaultPrinter();
Console.WriteLine($"Enter Printer Name (Default={defaultPrinter}):");
var printerName = Console.ReadLine();
if (string.IsNullOrEmpty(printerName))
    printerName = defaultPrinter;

try
{
    if (string.IsNullOrEmpty(filePath))
        throw new ArgumentNullException(nameof(filePath));

    Console.WriteLine($"Sending output to '{printerName}'");
    PrintTextOverDocument.Print.PrintDocument(filePath, printerName, false, "#-SAMPLE HEADER-#", "#-SAMPLE FOOTER-#");
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
}

Console.WriteLine("Program complete.");
Console.ReadKey();
