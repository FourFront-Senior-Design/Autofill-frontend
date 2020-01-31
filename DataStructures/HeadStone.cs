using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class Headstone
    {
        public string SequenceID { get; set; }
        public string PrimaryKey { get; set; }
        public string CemeteryName { get; set; }
        public string BurialSectionNumber { get; set; }
        public string WallID { get; set; }
        public string RowNum { get; set; }
        public string GavestoneNumber { get; set; }
        public string MarkerType { get; set; }
        public string Emblem1 { get; set; }
        public string Emblem2 { get; set; }

        public Person PrimaryDecedent { get; set; }
        public List<Person> OthersDecedentList { get; set; }

        public string Image1FilePath { get; set; }
        public string Image2FilePath { get; set; }
        public string Image1FileName { get; set; }
        public string Image2FileName { get; set; }
    }

}
