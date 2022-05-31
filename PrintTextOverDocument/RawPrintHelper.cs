using System.Runtime.InteropServices; //For RawPrinterHelper, DllImport
using System.Text;

namespace PrintTextOverDocument
{
    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        #region dll Wrappers
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int size);

        #endregion dll Wrappers

        #region Methods

        public static string GetDefaultPrinter()
        {
            var sb = new StringBuilder();
            int size = 100;
            GetDefaultPrinter(sb, ref size);
            return sb.ToString();
        }


        /// <summary>
        /// This function gets the pdf file name.
        /// This function opens the pdf file, gets all its bytes & send them to print.
        /// </summary>
        /// <param name="szPrinterName">Printer Name</param>
        /// <param name="szFileName">Pdf File Name</param>
        /// <returns>true on success, false on failure</returns>
        public static bool SendFileToPrinter(string fileName, string printerName)
        {
            bool success = false;

            // Open the PDF file.
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Send the stream to the printer.
                success = SendStreamToPrinter(fs, printerName, Path.GetFileName(fileName));
            }
            return success;
        }

        public static bool SendStreamToPrinter(Stream stream, string printerName, string jobName = "Streamed Document")
        {
            bool success = false;
            Byte[] bytes = new Byte[stream.Length];
            stream.Position = 0;

            // Create a BinaryReader on the stream.
            using (BinaryReader br = new BinaryReader(stream))
            {
                // Unmanaged pointer.
                IntPtr ptrUnmanagedBytes = new IntPtr(0);
                int nLength = Convert.ToInt32(stream.Length);
                // Read contents of the stream into the array.
                bytes = br.ReadBytes(nLength);
                // Allocate some unmanaged memory for those bytes.
                ptrUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                // Copy the managed byte array into the unmanaged array.
                Marshal.Copy(bytes, 0, ptrUnmanagedBytes, nLength);
                // Send the unmanaged bytes to the printer.
                success = SendBytesToPrinter(printerName, ptrUnmanagedBytes, nLength, jobName);
                // Free the unmanaged memory that you allocated earlier.
                Marshal.FreeCoTaskMem(ptrUnmanagedBytes);
            }
            return success;
        }

        /// <summary>
        /// This function gets the printer name and an unmanaged array of bytes, the function sends those bytes to the print queue.
        /// </summary>
        /// <param name="printerName">Printer Name</param>
        /// <param name="pBytes">No. of bytes in the pdf file</param>
        /// <param name="dwCount">Word count</param>
        /// <returns>True on success, false on failure</returns>
        private static bool SendBytesToPrinter(string printerName, IntPtr pBytes, Int32 dwCount, string jobName)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool success = false; // Assume failure unless you specifically succeed.

            di.pDocName = jobName;
            //di.pDataType = "RAW";
            di.pDataType = "XPS_PASS";

            // Open the printer.
            if (OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write the bytes.
                        success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            // If print did not succeed, GetLastError may give more information about the failure.
            if (success == false)
            {
                dwError = Marshal.GetLastWin32Error();
                if (dwError != null)
                    throw Marshal.GetExceptionForHR(dwError);
            }
            return success;
        }
        #endregion Methods
    }
}

