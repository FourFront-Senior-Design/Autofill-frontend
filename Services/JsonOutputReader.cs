using DataStructures;
using ServicesInterface;
using Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Services
{
    public class JsonOutputReader: IOutputReader
    {
        private IDatabaseService _database;
        private JsonKeys _jsonKeys;
        private List<List<string>> _allKeys = new List<List<string>>();

        private bool _DeleteTempFilesDirectory = true;

        public JsonOutputReader(IDatabaseService database)
        {
            _database = database;
            _jsonKeys = new JsonKeys();

            // Set up AllKeys nested list
            _allKeys = new List<List<string>>();
            _allKeys.Add(_jsonKeys.EmptyList);
            _allKeys.Add(_jsonKeys.PrimaryKeys);
            _allKeys.Add(_jsonKeys.SecondKeys);
            _allKeys.Add(_jsonKeys.ThirdKeys);
            _allKeys.Add(_jsonKeys.FourthKeys);
            _allKeys.Add(_jsonKeys.FifthKeys);
            _allKeys.Add(_jsonKeys.SixthKeys);
            _allKeys.Add(_jsonKeys.SeventhKeys);
        }

        public void FillDatabase()
        {
            // Autofill marker type and other data from .tmp files
            Headstone currentHeadstone;
            int totalUprights = 0;
            int totalFlats = 0;

            for (int i = 1; i <= _database.TotalItems; i++)
            {
                bool updateDB = false;
                currentHeadstone = _database.GetHeadstone(i);

                // Set WallID to 0
                if (string.IsNullOrEmpty(currentHeadstone.WallID))
                {
                    currentHeadstone.WallID = "0";
                    updateDB = true;
                }

                // check if 2nd image exists in database
                if (string.IsNullOrWhiteSpace(currentHeadstone.Image2FileName))
                {
                    // Flat markers
                    totalFlats++;
                    if (string.IsNullOrWhiteSpace(currentHeadstone.MarkerType))
                    {
                        currentHeadstone.MarkerType = "Flat Marker";
                        updateDB = true;
                    }
                }
                else
                {
                    // Upright headstones
                    totalUprights++;
                    if (string.IsNullOrWhiteSpace(currentHeadstone.MarkerType))
                    {
                        currentHeadstone.MarkerType = "Upright Headstone";
                        updateDB = true;
                    }
                }

                Dictionary<string, string> tmpData = ReadTmpFile(currentHeadstone.Image1FileName);
                updateDB = UpdateHeadstone(ref currentHeadstone, tmpData);

                if (updateDB)
                {
                    _database.SetHeadstone(i, currentHeadstone);
                }
            }
            // delete tempFiles directory
            if (_DeleteTempFilesDirectory)
            {
                string tempFilesPath = _database.SectionFilePath + "\\tempFiles\\";
                Directory.Delete(tempFilesPath, true);
            }

            // write totalFlats and totalUprights to file "MarkerTypeSummary.txt"
            StreamWriter writer = new StreamWriter(_database.SectionFilePath + "\\MarkerTypeSummary.txt");
            writer.Write("Uprights: " + totalUprights.ToString());
            writer.Write("\nFlats: " +  totalFlats.ToString());
            writer.Close();
        }

        private Dictionary<string, string> ReadTmpFile(string filename)
        {
            // Private internal function to read file into Dictionary
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Encoding encoding = Encoding.UTF8;
            string result;

            // Set up filename
            string path = _database.SectionFilePath + "\\tempFiles\\"+ filename;
            // Replace .jpg extension from file name
            path = path.Remove(path.Length - 4, 4);
            path += ".tmp";
            using (StreamReader streamReader = new StreamReader(path, encoding))
            {
                result = streamReader.ReadToEnd();
            }
            // Read .tmp file into string - convert to list
            List<string> tmpList = new List<string>(result.Split('\n'));

            // Set up dictionary of key,value pairs from file
            foreach (string item in tmpList)
            {
                // Add only key,value pairs
                string[] line = item.Split(':');
                if (line.Length == 2)
                {
                    dict.Add(line[0], line[1].Trim('\r'));
                }
                else if (line.Length == 1 && !string.IsNullOrEmpty(line[0]))
                {
                    dict.Add(line[0], null);
                }
            }

            return dict;
        }

        private bool UpdateHeadstone(ref Headstone h, Dictionary<string, string> tmpData)
        {
            // Write data to the Headstone - no overwrite of existing data
            // NOTE: This code needs to be refactored for multiple reasons:
            // 1) The access to non-primary decedents is difficult
            // 2) Convert from multiple if statements to a loop if its possible
            //    to iterate through the Headstone fields in order
            // Primary  
            bool updateDB = false;
            string value;

            if (tmpData.TryGetValue("First Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.FirstName))
            {
                h.PrimaryDecedent.FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Middle Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.MiddleName))
            {
                h.PrimaryDecedent.MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Last Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.LastName))
            {
                h.PrimaryDecedent.LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Suffix", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Suffix))
            {
                h.PrimaryDecedent.Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Location", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Location))
            {
                h.PrimaryDecedent.Location = value.Trim('\r');
                updateDB = true;
            }
            List<string> Ranks = new List<string>() { "Rank", "Rank2", "Rank3" };
            for (int i = 0; i < 3; i++)
            {
                if (tmpData.TryGetValue(Ranks[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.RankList[i]))
                {
                    h.PrimaryDecedent.RankList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            List<string> Branches = new List<string>() { "Branch", "Branch2", "Branch3" };
            for (int i = 0; i < 3; i++)
            {
                if (tmpData.TryGetValue(Branches[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BranchList[i]))
                {
                    h.PrimaryDecedent.BranchList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("Branch-Unit_CustomV", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BranchUnitCustom))
            {
                h.PrimaryDecedent.BranchUnitCustom = value.Trim('\r');
                updateDB = true;
            }
            List<string> Wars = new List<string>() { "War", "War2", "War3", "War4" };
            for (int i = 0; i < 4; i++)
            {
                if (tmpData.TryGetValue(Wars[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.WarList[i]))
                {
                    h.PrimaryDecedent.WarList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("BirthDate", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BirthDate))
            {
                h.PrimaryDecedent.BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDate", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.DeathDate))
            {
                h.PrimaryDecedent.DeathDate = value.Trim('\r');
                updateDB = true;
            }
            List<string> Awards = new List<string>() { "Award", "Award2", "Award3", "Award4", "Award5", "Award6", "Award7" };
            for (int i = 0; i < 7; i++)
            {
                if (tmpData.TryGetValue(Awards[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.AwardList[i]))
                {
                    h.PrimaryDecedent.AwardList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("Awards_Custom", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.AwardCustom))
            {
                h.PrimaryDecedent.AwardCustom = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Inscription", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Inscription))
            {
                h.PrimaryDecedent.Inscription = value.Trim('\r');
                updateDB = true;
            }
            // Secondary 
            if (tmpData.TryGetValue("First Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].FirstName))
            {
                h.OthersDecedentList[0].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Middle Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].MiddleName))
            {
                h.OthersDecedentList[0].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Last Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].LastName))
            {
                h.OthersDecedentList[0].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("Suffix Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Suffix))
            {
                h.OthersDecedentList[0].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Location))
            {
                h.OthersDecedentList[0].Location = value.Trim('\r');
                updateDB = true;
            }
            List<string> RankSD = new List<string>() { "RankS_D", "Rank2S_D", "Rank3S_D" };
            for (int i = 0; i < 3; i++)
            {
                if (tmpData.TryGetValue(RankSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].RankList[i]))
                {
                    h.OthersDecedentList[0].RankList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            List<string> BranchSD = new List<string>() { "Branch", "Branch2", "Branch3" };
            for (int i = 0; i < 3; i++)
            {
                if (tmpData.TryGetValue(BranchSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BranchList[i]))
                {
                    h.OthersDecedentList[0].BranchList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("Branch-Unit_CustomS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BranchUnitCustom))
            {
                h.OthersDecedentList[0].BranchUnitCustom = value.Trim('\r');
                updateDB = true;
            }
            List<string> WarSD = new List<string>() { "WarS_D", "War2S_D", "War3S_D", "War4S_D" };
            for (int i = 0; i < 4; i++)
            {
                if (tmpData.TryGetValue(WarSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].WarList[i]))
                {
                    h.OthersDecedentList[0].WarList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("BirthDateS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BirthDate))
            {
                h.OthersDecedentList[0].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].DeathDate))
            {
                h.OthersDecedentList[0].DeathDate = value.Trim('\r');
                updateDB = true;
            }
            List<string> AwardSD = new List<string>() { "AwardS_D", "Award2S_D", "Award3S_D", "Award4S_D", "Award5S_D", "Award6S_D", "Award7S_D" };
            for (int i = 0; i < 7; i++)
            {
                if (tmpData.TryGetValue(AwardSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].AwardList[i]))
                {
                    h.OthersDecedentList[0].AwardList[i] = value.Trim('\r');
                    updateDB = true;
                }
            }
            if (tmpData.TryGetValue("Awards_CustomS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].AwardCustom))
            {
                h.OthersDecedentList[0].AwardCustom = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("InscriptionS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Inscription))
            {
                h.OthersDecedentList[0].Inscription = value.Trim('\r');
                updateDB = true;
            }
            // Third
            if (tmpData.TryGetValue("FirstNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].FirstName))
            {
                h.OthersDecedentList[1].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("MiddleNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].MiddleName))
            {
                h.OthersDecedentList[1].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LastNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].LastName))
            {
                h.OthersDecedentList[1].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("SuffixS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Suffix))
            {
                h.OthersDecedentList[1].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Location))
            {
                h.OthersDecedentList[1].Location = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("RankS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].RankList[0]))
            {
                h.OthersDecedentList[1].RankList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BranchS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].BranchList[0]))
            {
                h.OthersDecedentList[1].BranchList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("WarS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].WarList[0]))
            {
                h.OthersDecedentList[1].WarList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BirthDateS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].BirthDate))
            {
                h.OthersDecedentList[1].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].DeathDate))
            {
                h.OthersDecedentList[1].DeathDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("AwardS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].AwardList[0]))
            {
                h.OthersDecedentList[1].AwardList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("InscriptionS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Inscription))
            {
                h.OthersDecedentList[1].Inscription = value.Trim('\r');
                updateDB = true;
            }
            // Fourth
            if (tmpData.TryGetValue("FirstNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].FirstName))
            {
                h.OthersDecedentList[2].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("MiddleNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].MiddleName))
            {
                h.OthersDecedentList[2].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LastNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].LastName))
            {
                h.OthersDecedentList[2].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("SuffixS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Suffix))
            {
                h.OthersDecedentList[2].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Location))
            {
                h.OthersDecedentList[2].Location = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("RankS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].RankList[0]))
            {
                h.OthersDecedentList[2].RankList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BranchS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].BranchList[0]))
            {
                h.OthersDecedentList[2].BranchList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("WarS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].WarList[0]))
            {
                h.OthersDecedentList[2].WarList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BirthDateS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].BirthDate))
            {
                h.OthersDecedentList[2].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].DeathDate))
            {
                h.OthersDecedentList[2].DeathDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("AwardS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].AwardList[0]))
            {
                h.OthersDecedentList[2].AwardList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("InscriptionS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Inscription))
            {
                h.OthersDecedentList[2].Inscription = value.Trim('\r');
                updateDB = true;
            }
            // Fifth
            if (tmpData.TryGetValue("FirstNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].FirstName))
            {
                h.OthersDecedentList[3].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("MiddleNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].MiddleName))
            {
                h.OthersDecedentList[3].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LastNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].LastName))
            {
                h.OthersDecedentList[3].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("SuffixS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Suffix))
            {
                h.OthersDecedentList[3].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Location))
            {
                h.OthersDecedentList[3].Location = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("RankS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].RankList[0]))
            {
                h.OthersDecedentList[3].RankList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BranchS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].BranchList[0]))
            {
                h.OthersDecedentList[3].BranchList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("WarS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].WarList[0]))
            {
                h.OthersDecedentList[3].WarList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BirthDateS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].BirthDate))
            {
                h.OthersDecedentList[3].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].DeathDate))
            {
                h.OthersDecedentList[3].DeathDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("AwardS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].AwardList[0]))
            {
                h.OthersDecedentList[3].AwardList[0] = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("InscriptionS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Inscription))
            {
                h.OthersDecedentList[3].Inscription = value.Trim('\r');
                updateDB = true;
            }
            // Sixth
            if (tmpData.TryGetValue("FirstNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].FirstName))
            {
                h.OthersDecedentList[4].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("MiddleNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].MiddleName))
            {
                h.OthersDecedentList[4].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LastNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].LastName))
            {
                h.OthersDecedentList[4].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("SuffixS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].Suffix))
            {
                h.OthersDecedentList[4].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].Location))
            {
                h.OthersDecedentList[4].Location = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BirthDateS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].BirthDate))
            {
                h.OthersDecedentList[4].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].DeathDate))
            {
                h.OthersDecedentList[4].DeathDate = value.Trim('\r');
                updateDB = true;
            }
            // Seventh
            if (tmpData.TryGetValue("FirstNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].FirstName))
            {
                h.OthersDecedentList[5].FirstName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("MiddleNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].MiddleName))
            {
                h.OthersDecedentList[5].MiddleName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LastNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].LastName))
            {
                h.OthersDecedentList[5].LastName = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("SuffixS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].Suffix))
            {
                h.OthersDecedentList[5].Suffix = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("LocationS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].Location))
            {
                h.OthersDecedentList[5].Location = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("BirthDateS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].BirthDate))
            {
                h.OthersDecedentList[5].BirthDate = value.Trim('\r');
                updateDB = true;
            }
            if (tmpData.TryGetValue("DeathDateS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].DeathDate))
            {
                h.OthersDecedentList[5].DeathDate = value.Trim('\r');
                updateDB = true;
            }

            return updateDB;
        }

    }
}
