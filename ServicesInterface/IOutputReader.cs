using System.Collections.Generic;

namespace ServicesInterface
{
    public interface IOutputReader
    {
        // reads in the extracted texts into the database
        int FillDatabase();
    }
}
