using System;
using System.Diagnostics;
using ServicesInterface;

namespace Services
{
    public class AutofillController : IAutofillController
    {
        // Runs the controller that runs the scripts to run google vision
        // and create the tempfiles with the extracted texts
        public void RunScripts(string filePath)
        {
            String pythonInstallation = @"C:\Python";

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonInstallation + @"\python.exe";
            start.Arguments = pythonInstallation + @"\FourFrontScripts\controller.py "
                + "\"" + filePath + "\"";
            
            start.UseShellExecute = false;
            start.CreateNoWindow = true;

            //Trace.WriteLine(start.FileName);
            //Trace.WriteLine(start.Arguments);

            using (Process process = new Process())
            {
                process.StartInfo = start;
                process.Start();
                process.WaitForExit();
            }
            Trace.WriteLine("Scripts ran successfully");
        }
    }
}
