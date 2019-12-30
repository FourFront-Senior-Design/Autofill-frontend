using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesInterface
{
    public interface IOCRService
    {
        // Create a cache of JSON files with the extracted text.
        bool extractText(string filePath);
    }
}
