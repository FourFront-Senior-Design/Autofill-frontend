using System;
using ServicesInterface;
using System.Diagnostics;
using System.IO;

namespace Services
{
    public class GoogleVision : IOCRService
    {
        public bool extractText(string filePath)
        {
            String pythonInstallation = @"C:\Python";
            String outputDirectory = filePath + @"\GoogleVisionData";
            String completePath = filePath + @"\ReferencedImages";

            if (Directory.Exists(completePath))
            {
                Directory.CreateDirectory(outputDirectory);

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonInstallation + @"\python.exe";
                start.Arguments = pythonInstallation + @"\FourFrontScripts\googleVisionOCR.py "
                    + filePath + @"\ReferencedImages"
                    + " " + outputDirectory;

                start.RedirectStandardOutput = true;
                start.UseShellExecute = false;
                start.CreateNoWindow = true;

                Trace.WriteLine(start.FileName);
                Trace.WriteLine(start.Arguments);

                using (Process process = new Process())
                {
                    process.StartInfo = start;
                    process.Start();
                    process.WaitForExit();
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
