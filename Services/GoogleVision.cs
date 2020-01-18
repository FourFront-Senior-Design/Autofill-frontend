using System;
using System.IO;
using IronPython.Hosting;
using ServicesInterface;

namespace Services
{
    public class GoogleVision : IOCRService
    {
        public bool extractText(string filePath)
        {
            string completePath = filePath + "\\ReferencedImages";

            Console.WriteLine(completePath);

            if (Directory.Exists(completePath))
            {
                // Run GoogleVision script on the complete path and store in cache location
                var engine = Python.CreateEngine();
                var source = engine.CreateScriptSourceFromFile(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "FourFrontScripts", "googleVisionOCR.py"));

                var scope = engine.CreateScope();

                engine.GetSysModule().SetVariable("filePath", filePath);
                engine.GetSysModule().SetVariable("outPath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OCRCache\\"));

                source.Execute(scope);

                return true;
            }
            else
                throw new Exception("ReferencedImages directory doesn't exist.");
        }
    }
}
