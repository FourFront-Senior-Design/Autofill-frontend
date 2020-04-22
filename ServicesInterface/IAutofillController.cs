using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesInterface
{
    public interface IAutofillController
    {
        // Runs google vision and the autofill scripts
        void RunScripts(string filePath);
    }
}
