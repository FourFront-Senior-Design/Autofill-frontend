using System;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    public class Person
    {
        private string _firstName;
        private string _middleName;
        private string _lastName;
        private string _suffix;
        private string _location;
        private List<string> _rankList;
        private List<String> _awardList;
        private string _awardCustum;
        private List<String> _warList;
        private List<String> _branchList;
        private string _branchUnitCustom;
        private string _birthDate;
        private string _deathDate;
        private string _inscription;


        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _firstName = makeValid(value);
            }
        }

        public string MiddleName
        {
            get { return _middleName; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _middleName = makeValid(value);
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _lastName = makeValid(value);
            }
        }

        public string Suffix
        {
            get { return _suffix; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _suffix = makeValid(value);
            }
        }

        public string Location
        {
            get { return _location; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _location = makeValid(value);
            }
        }

        public List<string> RankList
        {
            get { return _rankList; }

            set
            {
                if (value == null)
                {
                    return;
                }

                _rankList = new List<string>();
                foreach (string rank in value)
                {
                    AddRank(rank);
                }
            }
        }

        public bool AddRank(string rank)
        {
            if (rank == null)
                return false;

            _rankList.Add(makeValid(rank));
            return true;
        }

        public List<string> AwardList
        {
            get { return _awardList; }

            set
            {
                if (value == null)
                {
                    return;
                }

                _awardList = new List<string>();
                foreach (string award in value)
                {
                    AddAward(award);
                }
            }
        }

        public bool AddAward(string award)
        {
            if (award == null)
                return false;

            _awardList.Add(makeValid(award));
            return true;
        }

        public string AwardCustom
        {
            get { return _awardCustum; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _awardCustum = makeValid(value);
            }
        }

        public List<string> WarList
        {

            get { return _warList; }

            set
            {
                if (value == null)
                {
                    return;
                }

                _warList = new List<string>();
                foreach (string war in value)
                {
                    AddWar(war);
                }
            }
        }

        public bool AddWar(string war)
        {
            if (war == null)
                return false;

            _warList.Add(makeValid(war));
            return true;
        }

        public List<string> BranchList
        {
            get { return _branchList; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _branchList = new List<string>();
                foreach (string branch in value)
                {
                    AddBranch(branch);
                }
            }
        }

        public bool AddBranch(string branch)
        {
            if (branch == null)
                return false;

            _branchList.Add(makeValid(branch));
            return true;
        }

        public string BranchUnitCustom
        {
            get { return _branchUnitCustom; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _branchUnitCustom = makeValid(value);
            }
        }

        public string BirthDate
        {
            get { return _birthDate; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _birthDate = makeValid(value);
            }
        }

        public string DeathDate
        {
            get { return _deathDate; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _deathDate = makeValid(value);
            }
        }

        public string Inscription
        {
            get { return _inscription; }
            set
            {
                if (value == null)
                {
                    return;
                }

                _inscription = makeValid(value);
            }
        }

        public string makeValid(string input)
        {
            input = input.Trim();
            input = input.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            return input;
        }

        public Person()
        {
            _rankList = new List<string>();
            _warList = new List<string>();
            _awardList = new List<string>();
            _branchList = new List<string>();
            clearPerson();
        }

        public void clearPerson()
        {
            _firstName = "";
            _middleName = "";
            _lastName = "";
            _suffix = "";
            _location = "";
            _rankList.Clear();
            _awardList.Clear();
            _awardCustum = "";
            _warList.Clear();
            _branchList.Clear();
            _branchUnitCustom = "";
            _birthDate = "";
            _deathDate = "";
            _inscription = "";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Person person))
            {
                return false;
            }


            if (FirstName == person.FirstName &&
                MiddleName == person.MiddleName &&
                LastName == person.LastName &&
                Suffix == person.Suffix &&
                Location == person.Location &&
                BirthDate == person.BirthDate &&
                DeathDate == person.DeathDate &&
                BranchUnitCustom == person.BranchUnitCustom &&
                AwardCustom == person.AwardCustom &&
                Inscription == person.Inscription &&
                AwardList.SequenceEqual(person.AwardList) &&
                WarList.SequenceEqual(person.WarList) &&
                RankList.SequenceEqual(person.RankList) &&
                BranchList.SequenceEqual(person.BranchList))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
