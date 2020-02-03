using Common;
using DataStructures;
using NLog;
using ServicesInterface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Services
{
    public class MicrosoftAccess : IDatabaseService
    {
        private string _connectionString;

        private List<string> SequenceIDs { get; set; }

        public int TotalItems { get; private set; }

        public List<CemeteryNameData> CemeteryNames { get; private set; }

        public List<LocationData> LocationNames { get; private set; }

        public List<BranchData> BranchNames { get; private set; }

        public List<WarData> WarNames { get; private set; }

        public List<AwardData> AwardNames { get; private set; }

        public string SectionFilePath { get; private set; } = string.Empty;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool InitDBConnection(string sectionFilePath)
        {
            SectionFilePath = sectionFilePath;


            GetAccessFilePath();
            TestFileAccess();

            TotalItems = GetTotalRecords();

            CemeteryNames = GetCemeteryData();
            LocationNames = GetLocationData();
            BranchNames = GetBranchData();
            AwardNames = GetAwardData();
            WarNames = GetWarData();
            SequenceIDs = GetSequenceIDs();

            return true;
        }

        private void GetAccessFilePath()
        {

            Regex reg = new Regex(@".*_be.accdb");

            try
            {
                var Dbfiles = Directory.GetFiles(SectionFilePath)
                                        .Where(path => reg.IsMatch(path))
                                        .ToList();

                // set the connection string
                _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Dbfiles[0];
            }
            catch (Exception e)
            {
                var errorMessage = $"Error Accessing MS Access File with path: {SectionFilePath}";
                ThrowAndLogArgumentException(errorMessage);
            }
        }

        private void TestFileAccess()
        {
            try
            {
                // create the db connection
                using (OleDbConnection connection = new OleDbConnection(_connectionString))
                // using to ensure connection is closed when we are done
                {
                    connection.Open(); // try to open the connection
                }
            }
            catch (Exception e)
            {
                var errorMessage = $"Error Accessing MS Access Database this is likely due to a mismatch in database drivers\n" +
                    $"Please ensure you have the Microsoft Access Database Engine (x64) 2010 Redistributable" +
                    $" by going to this link:https://www.microsoft.com/en-us/download/details.aspx?id=54920 " +
                    $"and selecting the x64 bit version.";
                ThrowAndLogArgumentException(errorMessage);
            }

        }

        private int GetTotalRecords()
        {
            string sqlQuery = "SELECT COUNT(SequenceID) FROM Master";
            OleDbCommand cmd;
            OleDbDataReader reader;
            int totalRecords = 0;

            using (OleDbConnection connection = new OleDbConnection(_connectionString)) // using to ensure connection is closed when we are done
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open(); // try to open the connection
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    totalRecords = reader.GetInt32(0);
                }
                catch (Exception e)
                {
                    ThrowAndLogArgumentException("Error getting total record count", e);
                }
            }

            return totalRecords;
        }

        public Headstone GetHeadstone(int index)
        {
            string sqlQuery = "SELECT * FROM Master WHERE SequenceID = \"" + SequenceIDs[index - 1] + "\"";
            Headstone headstone = new Headstone();

            try
            {
                var dataRow = GetDataRow(sqlQuery);
                headstone.SequenceID = dataRow[(int)MasterTableCols.SequenceID].ToString();
                headstone.PrimaryKey = dataRow[(int)MasterTableCols.PrimaryKey].ToString();
                headstone.CemeteryName = dataRow[(int)MasterTableCols.CemeteryName].ToString();
                headstone.BurialSectionNumber = dataRow[(int)MasterTableCols.BurialSectionNumber].ToString();
                headstone.WallID = dataRow[(int)MasterTableCols.Wall].ToString();
                headstone.RowNum = dataRow[(int)MasterTableCols.RowNumber].ToString();
                headstone.GavestoneNumber = dataRow[(int)MasterTableCols.GravesiteNumber].ToString();
                headstone.MarkerType = dataRow[(int)MasterTableCols.MarkerType].ToString();
                headstone.Emblem1 = dataRow[(int)MasterTableCols.Emblem1].ToString();
                headstone.Emblem2 = dataRow[(int)MasterTableCols.Emblem2].ToString();

                headstone.PrimaryDecedent = GetPrimaryPerson(dataRow);

                headstone.OthersDecedentList = GetAddtionalDecedents(dataRow);

                Console.WriteLine((int)MasterTableCols.FrontFilename);
                headstone.Image1FilePath = dataRow[(int)MasterTableCols.FrontFilename].ToString();
                headstone.Image2FilePath = dataRow[(int)MasterTableCols.BackFilename].ToString();

                headstone.Image1FileName = Path.GetFileName(headstone.Image1FilePath);
                headstone.Image2FileName = Path.GetFileName(headstone.Image2FilePath);
            }
            catch (Exception e)
            {

                ThrowAndLogArgumentException($"Error getting headstone with SequenceID at ${index.ToString()}", e);
            }

            return headstone;
        }

        private void ThrowAndLogArgumentException(string errorMessage, Exception innerException = null)
        {
            if (innerException == null)
            {
                logger.Log(LogLevel.Error, errorMessage);
                throw new ArgumentException(errorMessage, innerException);
            }
            else
            {
                logger.Log(LogLevel.Error, errorMessage);
                throw new ArgumentException(errorMessage);
            }
        }

        private object[] GetDataRow(string sqlQuery)
        {
            OleDbCommand cmd;
            OleDbDataReader reader;
            object[] dataRow;


            using (OleDbConnection connection = new OleDbConnection(_connectionString)) // using to ensure connection is closed when we are done
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open(); // try to open the connection
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error accsessing Database");
                    throw e;
                }

                try
                {
                    reader = cmd.ExecuteReader();
                    reader.Read();

                    dataRow = new object[reader.FieldCount];
                    reader.GetValues(dataRow);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return dataRow;
        }

        private Person GetPrimaryPerson(object[] dataRow)
        {
            Person primaryPerson = new Person();

            primaryPerson.FirstName = dataRow[(int)MasterTableCols.FirstName].ToString();
            primaryPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleName].ToString();
            primaryPerson.LastName = dataRow[(int)MasterTableCols.LastName].ToString();
            primaryPerson.Suffix = dataRow[(int)MasterTableCols.Suffix].ToString();
            primaryPerson.Location = dataRow[(int)MasterTableCols.Location].ToString();

            primaryPerson.RankList.Add(dataRow[(int)MasterTableCols.Rank].ToString());
            primaryPerson.RankList.Add(dataRow[(int)MasterTableCols.Rank2].ToString());
            primaryPerson.RankList.Add(dataRow[(int)MasterTableCols.Rank3].ToString());

            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award2].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award3].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award4].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award5].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award6].ToString());
            primaryPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award7].ToString());

            primaryPerson.AwardCustom = dataRow[(int)MasterTableCols.Awards_Custom].ToString();

            primaryPerson.WarList.Add(dataRow[(int)MasterTableCols.War].ToString());
            primaryPerson.WarList.Add(dataRow[(int)MasterTableCols.War2].ToString());
            primaryPerson.WarList.Add(dataRow[(int)MasterTableCols.War3].ToString());
            primaryPerson.WarList.Add(dataRow[(int)MasterTableCols.War4].ToString());

            primaryPerson.BranchList.Add(dataRow[(int)MasterTableCols.Branch].ToString());
            primaryPerson.BranchList.Add(dataRow[(int)MasterTableCols.Branch2].ToString());
            primaryPerson.BranchList.Add(dataRow[(int)MasterTableCols.Branch3].ToString());

            primaryPerson.BranchUnitCustom = dataRow[(int)MasterTableCols.Branch_Unit_CustomV].ToString();

            primaryPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDate].ToString();
            primaryPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDate].ToString();

            primaryPerson.Inscription = dataRow[(int)MasterTableCols.Inscription].ToString();

            return primaryPerson;
        }

        private List<Person> GetAddtionalDecedents(object[] dataRow)
        {
            List<Person> additonalDecedents = new List<Person>();
            additonalDecedents.Add(GetSecondPerson(dataRow));
            additonalDecedents.Add(GetThirdPerson(dataRow));
            additonalDecedents.Add(GetForthPerson(dataRow));
            additonalDecedents.Add(GetFithPerson(dataRow));
            additonalDecedents.Add(GetSixthPerson(dataRow));
            additonalDecedents.Add(GetSeventhPerson(dataRow));

            return additonalDecedents;
        }

        private Person GetSecondPerson(object[] dataRow)
        {
            Person secondPerson = new Person();

            secondPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D].ToString();
            secondPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D].ToString();
            secondPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D].ToString();
            secondPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D].ToString();
            secondPerson.Location = dataRow[(int)MasterTableCols.LocationS_D].ToString();

            secondPerson.RankList.Add(dataRow[(int)MasterTableCols.RankS_D].ToString());
            secondPerson.RankList.Add(dataRow[(int)MasterTableCols.Rank2S_D].ToString());
            secondPerson.RankList.Add(dataRow[(int)MasterTableCols.Rank3S_D].ToString());

            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.AwardS_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award2S_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award3S_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award4S_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award5S_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award6S_D].ToString());
            secondPerson.AwardList.Add(dataRow[(int)MasterTableCols.Award7S_D].ToString());

            secondPerson.AwardCustom = dataRow[(int)MasterTableCols.Awards_CustomS_D].ToString();

            secondPerson.WarList.Add(dataRow[(int)MasterTableCols.WarS_D].ToString());
            secondPerson.WarList.Add(dataRow[(int)MasterTableCols.War2S_D].ToString());
            secondPerson.WarList.Add(dataRow[(int)MasterTableCols.War3S_D].ToString());
            secondPerson.WarList.Add(dataRow[(int)MasterTableCols.War4S_D].ToString());

            secondPerson.BranchList.Add(dataRow[(int)MasterTableCols.BranchS_D].ToString());
            secondPerson.BranchList.Add(dataRow[(int)MasterTableCols.Branch2S_D].ToString());
            secondPerson.BranchList.Add(dataRow[(int)MasterTableCols.Branch3S_D].ToString());

            secondPerson.BranchUnitCustom = dataRow[(int)MasterTableCols.Branch_Unit_CustomS_D].ToString();

            secondPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D].ToString();
            secondPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D].ToString();

            secondPerson.Inscription = dataRow[(int)MasterTableCols.InscriptionS_D].ToString();

            return secondPerson;
        }

        private Person GetThirdPerson(object[] dataRow)
        {
            Person thirdPerson = new Person();

            thirdPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D_2].ToString();
            thirdPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D_2].ToString();
            thirdPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D_2].ToString();
            thirdPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D_2].ToString();
            thirdPerson.Location = dataRow[(int)MasterTableCols.LocationS_D_2].ToString();

            thirdPerson.RankList.Add(dataRow[(int)MasterTableCols.RankS_D_2].ToString());

            thirdPerson.AwardList.Add(dataRow[(int)MasterTableCols.AwardS_D_2].ToString());

            thirdPerson.WarList.Add(dataRow[(int)MasterTableCols.WarS_D_2].ToString());

            thirdPerson.BranchList.Add(dataRow[(int)MasterTableCols.BranchS_D_2].ToString());

            thirdPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D_2].ToString();
            thirdPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D_2].ToString();

            thirdPerson.Inscription = dataRow[(int)MasterTableCols.InscriptionS_D_2].ToString();

            return thirdPerson;
        }

        private Person GetForthPerson(object[] dataRow)
        {
            Person forthPerson = new Person();

            forthPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D_3].ToString();
            forthPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D_3].ToString();
            forthPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D_3].ToString();
            forthPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D_3].ToString();
            forthPerson.Location = dataRow[(int)MasterTableCols.LocationS_D_3].ToString();

            forthPerson.RankList.Add(dataRow[(int)MasterTableCols.RankS_D_3].ToString());

            forthPerson.AwardList.Add(dataRow[(int)MasterTableCols.AwardS_D_3].ToString());

            forthPerson.WarList.Add(dataRow[(int)MasterTableCols.WarS_D_3].ToString());

            forthPerson.BranchList.Add(dataRow[(int)MasterTableCols.BranchS_D_3].ToString());

            forthPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D_3].ToString();
            forthPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D_3].ToString();

            forthPerson.Inscription = dataRow[(int)MasterTableCols.InscriptionS_D_3].ToString();

            return forthPerson;
        }

        private Person GetFithPerson(object[] dataRow)
        {
            Person fithPerson = new Person();

            fithPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D_4].ToString();
            fithPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D_4].ToString();
            fithPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D_4].ToString();
            fithPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D_4].ToString();
            fithPerson.Location = dataRow[(int)MasterTableCols.LocationS_D_4].ToString();

            fithPerson.RankList.Add(dataRow[(int)MasterTableCols.RankS_D_4].ToString());

            fithPerson.AwardList.Add(dataRow[(int)MasterTableCols.AwardS_D_4].ToString());

            fithPerson.WarList.Add(dataRow[(int)MasterTableCols.WarS_D_4].ToString());

            fithPerson.BranchList.Add(dataRow[(int)MasterTableCols.BranchS_D_4].ToString());

            fithPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D_4].ToString();
            fithPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D_4].ToString();

            fithPerson.Inscription = dataRow[(int)MasterTableCols.InscriptionS_D_4].ToString();

            return fithPerson;
        }

        private Person GetSixthPerson(object[] dataRow)
        {
            Person sixthPerson = new Person();

            sixthPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D_5].ToString();
            sixthPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D_5].ToString();
            sixthPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D_5].ToString();
            sixthPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D_5].ToString();
            sixthPerson.Location = dataRow[(int)MasterTableCols.LocationS_D_5].ToString();

            sixthPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D_5].ToString();
            sixthPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D_5].ToString();


            return sixthPerson;
        }

        private Person GetSeventhPerson(object[] dataRow)
        {
            Person seventhPerson = new Person();

            seventhPerson.FirstName = dataRow[(int)MasterTableCols.FirstNameS_D_6].ToString();
            seventhPerson.MiddleName = dataRow[(int)MasterTableCols.MiddleNameS_D_6].ToString();
            seventhPerson.LastName = dataRow[(int)MasterTableCols.LastNameS_D_6].ToString();
            seventhPerson.Suffix = dataRow[(int)MasterTableCols.SuffixS_D_6].ToString();
            seventhPerson.Location = dataRow[(int)MasterTableCols.LocationS_D_6].ToString();

            seventhPerson.BirthDate = dataRow[(int)MasterTableCols.BirthDateS_D_6].ToString();
            seventhPerson.DeathDate = dataRow[(int)MasterTableCols.DeathDateS_D_6].ToString();

            return seventhPerson;
        }

        private List<CemeteryNameData> GetCemeteryData()
        {
            List<CemeteryNameData> CemetaryData = new List<CemeteryNameData>();
            OleDbCommand cmd;
            OleDbDataReader reader;

            string sqlQuery = "SELECT * FROM CemeteryNames";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                cmd = new OleDbCommand(sqlQuery, connection);
                connection.Open();

                try
                {
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        CemeteryNameData data = new CemeteryNameData();

                        data.ID = reader.GetInt32(0);
                        data.CemeteryName = reader.GetString(1).ToUpper();
                        data.KeyName = reader.GetString(2).ToUpper();

                        CemetaryData.Add(data);
                    }
                    reader.Close();
                    connection.Close();

                }
                catch (Exception e)
                {
                    ThrowAndLogArgumentException("Error getting cemetery data", e);
                }
            }

            CemetaryData = CemetaryData.OrderBy(x => x.CemeteryName).ToList();
            return CemetaryData;
        }

        private List<AwardData> GetAwardData()
        {
            List<AwardData> AwardNames = new List<AwardData>();
            OleDbCommand cmd;
            OleDbDataReader reader;

            string sqlQuery = "SELECT CODE, AWARD FROM AwardList";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {

                cmd = new OleDbCommand(sqlQuery, connection);
                connection.Open();

                try
                {
                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        AwardData data = new AwardData();

                        data.Code = reader.GetString(0).ToUpper();
                        data.Award = reader.GetString(1).ToUpper();

                        AwardNames.Add(data);
                    }


                    reader.Close();
                    connection.Close();

                }
                catch (Exception e)
                {
                    ThrowAndLogArgumentException("Error getting award data", e);
                }
            }

            AwardNames = AwardNames.OrderBy(x => x.Code).ToList();
            return AwardNames;
        }

        private List<BranchData> GetBranchData()
        {
            List<BranchData> BranchNames = new List<BranchData>();
            OleDbCommand cmd;
            OleDbDataReader reader;

            string sqlQuery = "SELECT Code, [Branch of Service], [Short Description] FROM BranchList";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error accsessing Database");
                    throw e;
                }

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    BranchData data = new BranchData();

                    data.Code = reader.GetString(0).ToUpper();
                    data.BranchOfService = reader.GetString(1).ToUpper();
                    data.ShortDescription = reader.GetString(2).ToUpper();

                    BranchNames.Add(data);
                }


                reader.Close();
                connection.Close();
            }

            BranchNames = BranchNames.OrderBy(x => x.Code).ToList();
            return BranchNames;
        }

        private List<WarData> GetWarData()
        {
            List<WarData> WarNames = new List<WarData>();
            OleDbCommand cmd;
            OleDbDataReader reader;

            string sqlQuery = "SELECT Code, [Short Description] FROM WarList";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error accsessing Database");
                    throw e;
                }

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    WarData data = new WarData();

                    data.Code = reader.GetString(0).ToUpper();
                    data.ShortDescription = reader.GetString(1).ToUpper();

                    WarNames.Add(data);
                }


                reader.Close();
                connection.Close();
            }

            WarNames = WarNames.OrderBy(x => x.Code).ToList();
            return WarNames;
        }



        private List<LocationData> GetLocationData()
        {
            List<LocationData> LocationNames = new List<LocationData>();
            OleDbCommand cmd;
            OleDbDataReader reader;

            string sqlQuery = "SELECT ID, LocationAbbrev, Location FROM LocationList";

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error accsessing Database");
                    throw e;
                }


                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    LocationData data = new LocationData();

                    data.ID = reader.GetInt32(0);
                    data.Abbr = reader.GetString(1).ToUpper();
                    data.Location = reader.GetString(2).ToUpper();

                    LocationNames.Add(data);
                }


                reader.Close();
                connection.Close();
            }

            LocationNames = LocationNames.OrderBy(x => x.Location).ToList();
            return LocationNames;
        }

        private string getCemeteryKey(string cemeteryName)
        {
            foreach (CemeteryNameData cemetery in CemeteryNames)
            {
                if (cemetery.CemeteryName == cemeteryName)
                {
                    return cemetery.KeyName;
                }
            }
            return "";
        }
        
        private List<string> GetSequenceIDs()
        {
            List<string> sequenceIDs = new List<string>();

            using (OleDbConnection connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand cmd;
                OleDbDataReader reader;

                try
                {
                    connection.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Accessing Database");
                    throw e;
                }

                try
                {
                    string sqlQuery = "SELECT SequenceID FROM Master;";
                    cmd = new OleDbCommand(sqlQuery, connection);
                    reader = cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Querying for SequenceIDs");
                    throw e;
                }

                while (reader.Read())
                {
                    sequenceIDs.Add(reader.GetString(0));
                }

                reader.Close();
            }
            sequenceIDs.Sort();
            return sequenceIDs;
        }

        public void SetHeadstone(int index, Headstone headstone)
        {
            // For each field in headstone that has content, update the database
            Dictionary<string, string> headstoneData = new Dictionary<string, string>();

            SetHeader(ref headstoneData, ref headstone);
            SetPrimaryKey(ref headstoneData, ref headstone);
            SetPrimaryPerson(ref headstoneData, ref headstone);
            SetFirstPerson(ref headstoneData, ref headstone);
            SetSecondPerson(ref headstoneData, ref headstone);
            SetThirdPerson(ref headstoneData, ref headstone);
            SetFourthPerson(ref headstoneData, ref headstone);
            SetFifthPerson(ref headstoneData, ref headstone);
            SetSixthPerson(ref headstoneData, ref headstone);

            string sqlQuery = @"UPDATE Master SET ";

            // Append all keys and values to the string
            foreach (KeyValuePair<string, string> entry in headstoneData)
            {
                sqlQuery += @"[" + entry.Key + @"] = " + @"@" + entry.Key + @", ";
            }

            sqlQuery += "[Branch-Unit_CustomV] = '" + headstone.PrimaryDecedent.BranchUnitCustom +
                "', [Branch-Unit_CustomS_D] = '" + headstone.OthersDecedentList[0].BranchUnitCustom + "'";

            // finalize update statement
            //sqlQuery += @" WHERE SequenceID = '" + sequenceID[index] + @"';";
            sqlQuery += @" WHERE SequenceID = '" + SequenceIDs[index - 1] + "';";

            OleDbCommand cmd;
            using (OleDbConnection connection = new OleDbConnection(_connectionString)) // using to ensure connection is closed when we are done
            {
                try
                {
                    cmd = new OleDbCommand(sqlQuery, connection);
                    connection.Open(); // try to open the connection
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error accessing Database");
                    throw e;
                }

                string[] intEntries = { "Wall", "Emblem1", "Emblem2" };
                foreach (KeyValuePair<string, string> entry in headstoneData)
                {
                    try
                    {
                        if (intEntries.Contains(entry.Key))
                        {
                            if (entry.Value == "")
                                cmd.Parameters.AddWithValue("@" + entry.Key, OleDbType.Integer).Value = DBNull.Value;
                            else
                                cmd.Parameters.AddWithValue("@" + entry.Key, Convert.ToInt32(entry.Value));
                        }
                        else
                        {
                            if (entry.Value == "")
                                cmd.Parameters.AddWithValue("@" + entry.Key, OleDbType.VarChar).Value = DBNull.Value;
                            else
                                cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error with: " + entry.Key + " = " + entry.Value);
                        Console.WriteLine(e);
                    }
                }

                try
                {
                    cmd.ExecuteNonQuery(); // do the update
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error writing to the database:");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(cmd.CommandText);
                }
            }
        }

        private void SetHeader(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("CemeteryName", headstone.CemeteryName);
            dict.Add("BurialSectionNumber", headstone.BurialSectionNumber);
            dict.Add("Wall", headstone.WallID);
            dict.Add("RowNumber", headstone.RowNum);
            dict.Add("GravesiteNumber", headstone.GavestoneNumber);
            dict.Add("MarkerType", headstone.MarkerType);
            dict.Add("Emblem1", headstone.Emblem1);
            dict.Add("Emblem2", headstone.Emblem2);
        }

        private void SetPrimaryKey(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            string CemeteryKey = getCemeteryKey(headstone.CemeteryName);

            string PrimaryKey = CemeteryKey + "-" + headstone.BurialSectionNumber +
                "-" + headstone.RowNum + "-" + headstone.GavestoneNumber;

            dict.Add("PrimaryKey", PrimaryKey);
        }

        private void SetPrimaryPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstName", headstone.PrimaryDecedent.FirstName);
            dict.Add("MiddleName", headstone.PrimaryDecedent.MiddleName);
            dict.Add("LastName", headstone.PrimaryDecedent.LastName);
            dict.Add("Suffix", headstone.PrimaryDecedent.Suffix);
            dict.Add("Location", headstone.PrimaryDecedent.Location);

            dict.Add("Rank", headstone.PrimaryDecedent.RankList[0]);
            dict.Add("Rank2", headstone.PrimaryDecedent.RankList[1]);
            dict.Add("Rank3", headstone.PrimaryDecedent.RankList[2]);

            dict.Add("Award", headstone.PrimaryDecedent.AwardList[0]);
            dict.Add("Award2", headstone.PrimaryDecedent.AwardList[1]);
            dict.Add("Award3", headstone.PrimaryDecedent.AwardList[2]);
            dict.Add("Award4", headstone.PrimaryDecedent.AwardList[3]);
            dict.Add("Award5", headstone.PrimaryDecedent.AwardList[4]);
            dict.Add("Award6", headstone.PrimaryDecedent.AwardList[5]);
            dict.Add("Award7", headstone.PrimaryDecedent.AwardList[6]);

            dict.Add("Awards_Custom", headstone.PrimaryDecedent.AwardCustom);

            dict.Add("War", headstone.PrimaryDecedent.WarList[0]);
            dict.Add("War2", headstone.PrimaryDecedent.WarList[1]);
            dict.Add("War3", headstone.PrimaryDecedent.WarList[2]);
            dict.Add("War4", headstone.PrimaryDecedent.WarList[3]);

            dict.Add("Branch", headstone.PrimaryDecedent.BranchList[0]);
            dict.Add("Branch2", headstone.PrimaryDecedent.BranchList[1]);
            dict.Add("Branch3", headstone.PrimaryDecedent.BranchList[2]);

            dict.Add("BirthDate", headstone.PrimaryDecedent.BirthDate);
            dict.Add("DeathDate", headstone.PrimaryDecedent.DeathDate);

            dict.Add("Inscription", headstone.PrimaryDecedent.Inscription);
        }

        private void SetFirstPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D", headstone.OthersDecedentList[0].FirstName);
            dict.Add("MiddleNameS_D", headstone.OthersDecedentList[0].MiddleName);
            dict.Add("LastNameS_D", headstone.OthersDecedentList[0].LastName);
            dict.Add("SuffixS_D", headstone.OthersDecedentList[0].Suffix);
            dict.Add("LocationS_D", headstone.OthersDecedentList[0].Location);

            dict.Add("RankS_D", headstone.OthersDecedentList[0].RankList[0]);
            dict.Add("Rank2S_D", headstone.OthersDecedentList[0].RankList[1]);
            dict.Add("Rank3S_D", headstone.OthersDecedentList[0].RankList[2]);

            dict.Add("AwardS_D", headstone.OthersDecedentList[0].AwardList[0]);
            dict.Add("Award2S_D", headstone.OthersDecedentList[0].AwardList[1]);
            dict.Add("Award3S_D", headstone.OthersDecedentList[0].AwardList[2]);
            dict.Add("Award4S_D", headstone.OthersDecedentList[0].AwardList[3]);
            dict.Add("Award5S_D", headstone.OthersDecedentList[0].AwardList[4]);
            dict.Add("Award6S_D", headstone.OthersDecedentList[0].AwardList[5]);
            dict.Add("Award7S_D", headstone.OthersDecedentList[0].AwardList[6]);
            dict.Add("Awards_CustomS_D", headstone.OthersDecedentList[0].AwardCustom);

            dict.Add("WarS_D", headstone.OthersDecedentList[0].WarList[0]);
            dict.Add("War2S_D", headstone.OthersDecedentList[0].WarList[1]);
            dict.Add("War3S_D", headstone.OthersDecedentList[0].WarList[2]);
            dict.Add("War4S_D", headstone.OthersDecedentList[0].WarList[3]);

            dict.Add("BranchS_D", headstone.OthersDecedentList[0].BranchList[0]);
            dict.Add("Branch2S_D", headstone.OthersDecedentList[0].BranchList[1]);
            dict.Add("Branch3S_D", headstone.OthersDecedentList[0].BranchList[2]);

            dict.Add("BirthDateS_D", headstone.OthersDecedentList[0].BirthDate);
            dict.Add("DeathDateS_D", headstone.OthersDecedentList[0].DeathDate);

            dict.Add("InscriptionS_D", headstone.OthersDecedentList[0].Inscription);
        }

        private void SetSecondPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D_2", headstone.OthersDecedentList[1].FirstName);
            dict.Add("MiddleNameS_D_2", headstone.OthersDecedentList[1].MiddleName);
            dict.Add("LastNameS_D_2", headstone.OthersDecedentList[1].LastName);
            dict.Add("SuffixS_D_2", headstone.OthersDecedentList[1].Suffix);
            dict.Add("LocationS_D_2", headstone.OthersDecedentList[1].Location);

            dict.Add("RankS_D_2", headstone.OthersDecedentList[1].RankList[0]);
            dict.Add("AwardS_D_2", headstone.OthersDecedentList[1].AwardList[0]);
            dict.Add("WarS_D_2", headstone.OthersDecedentList[1].WarList[0]);
            dict.Add("BranchS_D_2", headstone.OthersDecedentList[1].BranchList[0]);

            dict.Add("InscriptionS_D_2", headstone.OthersDecedentList[1].Inscription);

            dict.Add("BirthDateS_D_2", headstone.OthersDecedentList[1].BirthDate);
            dict.Add("DeathDateS_D_2", headstone.OthersDecedentList[1].DeathDate);
        }

        private void SetThirdPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D_3", headstone.OthersDecedentList[2].FirstName);
            dict.Add("MiddleNameS_D_3", headstone.OthersDecedentList[2].MiddleName);
            dict.Add("LastNameS_D_3", headstone.OthersDecedentList[2].LastName);
            dict.Add("SuffixS_D_3", headstone.OthersDecedentList[2].Suffix);
            dict.Add("LocationS_D_3", headstone.OthersDecedentList[2].Location);

            dict.Add("RankS_D_3", headstone.OthersDecedentList[2].RankList[0]);
            dict.Add("AwardS_D_3", headstone.OthersDecedentList[2].AwardList[0]);
            dict.Add("WarS_D_3", headstone.OthersDecedentList[2].WarList[0]);
            dict.Add("BranchS_D_3", headstone.OthersDecedentList[2].BranchList[0]);

            dict.Add("InscriptionS_D_3", headstone.OthersDecedentList[2].Inscription);

            dict.Add("BirthDateS_D_3", headstone.OthersDecedentList[2].BirthDate);
            dict.Add("DeathDateS_D_3", headstone.OthersDecedentList[2].DeathDate);
        }

        private void SetFourthPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D_4", headstone.OthersDecedentList[3].FirstName);
            dict.Add("MiddleNameS_D_4", headstone.OthersDecedentList[3].MiddleName);
            dict.Add("LastNameS_D_4", headstone.OthersDecedentList[3].LastName);
            dict.Add("SuffixS_D_4", headstone.OthersDecedentList[3].Suffix);
            dict.Add("LocationS_D_4", headstone.OthersDecedentList[3].Location);

            dict.Add("RankS_D_4", headstone.OthersDecedentList[3].RankList[0]);
            dict.Add("AwardS_D_4", headstone.OthersDecedentList[3].AwardList[0]);
            dict.Add("WarS_D_4", headstone.OthersDecedentList[3].WarList[0]);
            dict.Add("BranchS_D_4", headstone.OthersDecedentList[3].BranchList[0]);

            dict.Add("InscriptionS_D_4", headstone.OthersDecedentList[3].Inscription);

            dict.Add("BirthDateS_D_4", headstone.OthersDecedentList[3].BirthDate);
            dict.Add("DeathDateS_D_4", headstone.OthersDecedentList[3].DeathDate);
        }

        private void SetFifthPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D_5", headstone.OthersDecedentList[4].FirstName);
            dict.Add("MiddleNameS_D_5", headstone.OthersDecedentList[4].MiddleName);
            dict.Add("LastNameS_D_5", headstone.OthersDecedentList[4].LastName);
            dict.Add("SuffixS_D_5", headstone.OthersDecedentList[4].Suffix);
            dict.Add("LocationS_D_5", headstone.OthersDecedentList[4].Location);

            dict.Add("BirthDateS_D_5", headstone.OthersDecedentList[4].BirthDate);
            dict.Add("DeathDateS_D_5", headstone.OthersDecedentList[4].DeathDate);
        }

        private void SetSixthPerson(ref Dictionary<string, string> dict, ref Headstone headstone)
        {
            dict.Add("FirstNameS_D_6", headstone.OthersDecedentList[5].FirstName);
            dict.Add("MiddleNameS_D_6", headstone.OthersDecedentList[5].MiddleName);
            dict.Add("LastNameS_D_6", headstone.OthersDecedentList[5].LastName);
            dict.Add("SuffixS_D_6", headstone.OthersDecedentList[5].Suffix);
            dict.Add("LocationS_D_6", headstone.OthersDecedentList[5].Location);

            dict.Add("BirthDateS_D_6", headstone.OthersDecedentList[5].BirthDate);
            dict.Add("DeathDateS_D_6", headstone.OthersDecedentList[5].DeathDate);
        }
    }
}
