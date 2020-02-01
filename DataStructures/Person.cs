using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    public class Person
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string Location { get; set; }
        public List<string> RankList { get; set; }
        public List<string> AwardList { get; set; }
        public string AwardCustom { get; set; }
        public List<string> WarList { get; set; }
        public List<string> BranchList { get; set; }
        public string BranchUnitCustom { get; set; }
        public string BirthDate { get; set; }
        public string DeathDate { get; set; }
        public string Inscription { get; set; }

        public Person()
        {
            RankList = new List<string>();
            AwardList = new List<string>();
            BranchList = new List<string>();
            WarList = new List<string>();
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
