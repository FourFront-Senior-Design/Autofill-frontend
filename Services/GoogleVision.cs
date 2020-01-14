using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                return true;
            }
            else
                throw new Exception("ReferencedImages directory doesn't exist.");
        }
    }
}
