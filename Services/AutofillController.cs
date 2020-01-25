using System;
using System.Diagnostics;
using ServicesInterface;

namespace Services
{
    public class AutofillController : IAutofillController
    {
        public void runScripts(string filePath)
        {
            String pythonInstallation = @"C:\Python";

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonInstallation + @"\python.exe";
            start.Arguments = pythonInstallation + @"\FourFrontScripts\controller.py "
                + filePath;

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
        }
    }
}
