using System;
using System.Collections.Generic;

namespace DataStructures
{
    public class Headstone
    {
        private string _cemeteryName;
        private string _burialSectionNumber;
        private string _wallID;
        private string _rowNum;
        private string _gravestoneNumber;
        private string _markerType;
        private string _emblem1;
        private string _emblem2;

        public string SequenceID { get; set; }
        public string PrimaryKey { get; set; }

        public string CemeteryName
        {
            get { return _cemeteryName; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _cemeteryName = makeValid(value);
            }
        }

        public string BurialSectionNumber
        {
            get { return _burialSectionNumber; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _burialSectionNumber = makeValid(value);
            }
        }

        public string WallID
        {
            get { return _wallID; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _wallID = makeValid(value);
            }
        }

        public string RowNum
        {
            get { return _rowNum; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _rowNum = makeValid(value);
            }
        }

        public string GavestoneNumber
        {
            get { return _gravestoneNumber; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _gravestoneNumber = makeValid(value);
            }
        }

        public string MarkerType
        {
            get { return _markerType; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _markerType = makeValid(value);
            }
        }

        public string Emblem1
        {
            get { return _emblem1; }
            set
            {
                if (value == null)
                {
                    return;
                }

                value = makeValid(value);
                if (isNumber(value) == false)
                {
                    _emblem1 = "";
                    return;
                }
                _emblem1 = value;
            }
        }

        public string Emblem2
        {
            get { return _emblem2; }
            set
            {
                if (value == null)
                {
                    return;
                }

                value = makeValid(value);
                if (isNumber(value) == false)
                {
                    _emblem2 = "";
                    return;
                }
                _emblem2 = value;
            }
        }

        public string makeValid(string input)
        {
            input = input.Trim();
            input = input.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            return input;
        }

        public bool isNumber(string input)
        {
            foreach (char ch in input)
            {
                if (!char.IsDigit(ch)) return false;
            }
            return true;
        }

        public Person PrimaryDecedent { get; set; }
        public List<Person> OthersDecedentList { get; set; }

        public string Image1FilePath { get; set; }
        public string Image2FilePath { get; set; }
        public string Image1FileName { get; set; }
        public string Image2FileName { get; set; }

        public Headstone()
        {
            CemeteryName = "";
            BurialSectionNumber = "";
            WallID = "";
            RowNum = "";
            GavestoneNumber = "";
            MarkerType = "";
            Emblem1 = "0";
            Emblem2 = "0";
        }
    }

}
