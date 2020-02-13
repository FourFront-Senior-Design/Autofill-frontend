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

                /* PRIMARY */
                headstone.fields["SequenceID"] = dataRow[(int)MasterTableCols.SequenceID].ToString();
                headstone.fields["PrimaryKey"] = dataRow[(int)MasterTableCols.PrimaryKey].ToString();
                headstone.fields["CemeteryName"] = dataRow[(int)MasterTableCols.CemeteryName].ToString();
                headstone.fields["BurialSectionNumber"] = dataRow[(int)MasterTableCols.BurialSectionNumber].ToString();
                headstone.fields["WallID"] = dataRow[(int)MasterTableCols.Wall].ToString();
                headstone.fields["RowNum"] = dataRow[(int)MasterTableCols.RowNumber].ToString();
                headstone.fields["GavestoneNumber"] = dataRow[(int)MasterTableCols.GravesiteNumber].ToString();
                headstone.fields["MarkerType"] = dataRow[(int)MasterTableCols.MarkerType].ToString();
                headstone.fields["Emblem1"] = dataRow[(int)MasterTableCols.Emblem1].ToString();
                headstone.fields["Emblem2"] = dataRow[(int)MasterTableCols.Emblem2].ToString();
                headstone.fields["FirstName"] = dataRow[(int)MasterTableCols.FirstName].ToString();
                headstone.fields["MiddleName"] = dataRow[(int)MasterTableCols.MiddleName].ToString();
                headstone.fields["LastName"] = dataRow[(int)MasterTableCols.LastName].ToString();
                headstone.fields["Suffix"] = dataRow[(int)MasterTableCols.Suffix].ToString();
                headstone.fields["Location"] = dataRow[(int)MasterTableCols.Location].ToString();
                headstone.fields["Rank"] = dataRow[(int)MasterTableCols.Rank].ToString();
                headstone.fields["Rank2"] = dataRow[(int)MasterTableCols.Rank2].ToString();
                headstone.fields["Rank3"] = dataRow[(int)MasterTableCols.Rank3].ToString();
                headstone.fields["Award"] = dataRow[(int)MasterTableCols.Award].ToString();
                headstone.fields["Award2"] = dataRow[(int)MasterTableCols.Award2].ToString();
                headstone.fields["Award3"] = dataRow[(int)MasterTableCols.Award3].ToString();
                headstone.fields["Award4"] = dataRow[(int)MasterTableCols.Award4].ToString();
                headstone.fields["Award5"] = dataRow[(int)MasterTableCols.Award5].ToString();
                headstone.fields["Award6"] = dataRow[(int)MasterTableCols.Award6].ToString();
                headstone.fields["Award7"] = dataRow[(int)MasterTableCols.Award7].ToString();
                headstone.fields["AwardCustom"] = dataRow[(int)MasterTableCols.Awards_Custom].ToString();
                headstone.fields["War"] = dataRow[(int)MasterTableCols.War].ToString();
                headstone.fields["War2"] = dataRow[(int)MasterTableCols.War2].ToString();
                headstone.fields["War3"] = dataRow[(int)MasterTableCols.War3].ToString();
                headstone.fields["War4"] = dataRow[(int)MasterTableCols.War4].ToString();
                headstone.fields["Branch"] = dataRow[(int)MasterTableCols.Branch].ToString();
                headstone.fields["Branch2"] = dataRow[(int)MasterTableCols.Branch2].ToString();
                headstone.fields["Branch3"] = dataRow[(int)MasterTableCols.Branch3].ToString();
                headstone.fields["BranchUnitCustom"] = dataRow[(int)MasterTableCols.Branch_Unit_CustomV].ToString();
                headstone.fields["BirthDate"] = dataRow[(int)MasterTableCols.BirthDate].ToString();
                headstone.fields["DeathDate"] = dataRow[(int)MasterTableCols.DeathDate].ToString();
                headstone.fields["Inscription"] = dataRow[(int)MasterTableCols.Inscription].ToString();

                /* SECONDARY */
                headstone.fields["FirstNameS_D"] = dataRow[(int)MasterTableCols.FirstNameS_D].ToString();
                headstone.fields["MiddleNameS_D"] = dataRow[(int)MasterTableCols.MiddleNameS_D].ToString();
                headstone.fields["LastNameS_D"] = dataRow[(int)MasterTableCols.LastNameS_D].ToString();
                headstone.fields["SuffixS_D"] = dataRow[(int)MasterTableCols.SuffixS_D].ToString();
                headstone.fields["LocationS_D"] = dataRow[(int)MasterTableCols.LocationS_D].ToString();
                headstone.fields["RankS_D"] = dataRow[(int)MasterTableCols.RankS_D].ToString();
                headstone.fields["Rank2S_D"] = dataRow[(int)MasterTableCols.Rank2S_D].ToString();
                headstone.fields["Rank3S_D"] = dataRow[(int)MasterTableCols.Rank3S_D].ToString();
                headstone.fields["AwardS_D"] = dataRow[(int)MasterTableCols.AwardS_D].ToString();
                headstone.fields["Award2S_D"] = dataRow[(int)MasterTableCols.Award2S_D].ToString();
                headstone.fields["Award3S_D"] = dataRow[(int)MasterTableCols.Award3S_D].ToString();
                headstone.fields["Award4S_D"] = dataRow[(int)MasterTableCols.Award4S_D].ToString();
                headstone.fields["Award5S_D"] = dataRow[(int)MasterTableCols.Award5S_D].ToString();
                headstone.fields["Award6S_D"] = dataRow[(int)MasterTableCols.Award6S_D].ToString();
                headstone.fields["Award7S_D"] = dataRow[(int)MasterTableCols.Award7S_D].ToString();
                headstone.fields["Awards_CustomS_D"] = dataRow[(int)MasterTableCols.Awards_CustomS_D].ToString();
                headstone.fields["WarS_D"] = dataRow[(int)MasterTableCols.WarS_D].ToString();
                headstone.fields["War2S_D"] = dataRow[(int)MasterTableCols.War2S_D].ToString();
                headstone.fields["War3S_D"] = dataRow[(int)MasterTableCols.War3S_D].ToString();
                headstone.fields["War4S_D"] = dataRow[(int)MasterTableCols.War4S_D].ToString();
                headstone.fields["BranchS_D"] = dataRow[(int)MasterTableCols.BranchS_D].ToString();
                headstone.fields["Branch2S_D"] = dataRow[(int)MasterTableCols.Branch2S_D].ToString();
                headstone.fields["Branch3S_D"] = dataRow[(int)MasterTableCols.Branch3S_D].ToString();
                headstone.fields["Branch_Unit_CustomS_D"] = dataRow[(int)MasterTableCols.Branch_Unit_CustomS_D].ToString();
                headstone.fields["BirthDateS_D"] = dataRow[(int)MasterTableCols.BirthDateS_D].ToString();
                headstone.fields["DeathDateS_D"] = dataRow[(int)MasterTableCols.DeathDateS_D].ToString();
                headstone.fields["InscriptionS_D"] = dataRow[(int)MasterTableCols.InscriptionS_D].ToString();
				
				/* THIRD */
				headstone.fields["FirstNameS_D_2"] = dataRow[(int)MasterTableCols.FirstNameS_D_2].ToString();
				headstone.fields["MiddleNameS_D_2"] = dataRow[(int)MasterTableCols.MiddleNameS_D_2].ToString();
				headstone.fields["LastNameS_D_2"] = dataRow[(int)MasterTableCols.LastNameS_D_2].ToString();
				headstone.fields["SuffixS_D_2"] = dataRow[(int)MasterTableCols.SuffixS_D_2].ToString();
				headstone.fields["LocationS_D_2"] = dataRow[(int)MasterTableCols.LocationS_D_2].ToString();
				headstone.fields["RankS_D_2"] = dataRow[(int)MasterTableCols.RankS_D_2].ToString();
				headstone.fields["AwardS_D_2"] = dataRow[(int)MasterTableCols.AwardS_D_2].ToString();
				headstone.fields["WarS_D_2"] = dataRow[(int)MasterTableCols.WarS_D_2].ToString();
				headstone.fields["BranchS_D_2"] = dataRow[(int)MasterTableCols.BranchS_D_2].ToString();
				headstone.fields["BirthDateS_D_2"] = dataRow[(int)MasterTableCols.BirthDateS_D_2].ToString();
				headstone.fields["DeathDateS_D_2"] = dataRow[(int)MasterTableCols.DeathDateS_D_2].ToString();
				headstone.fields["InscriptionS_D_2"] = dataRow[(int)MasterTableCols.InscriptionS_D_2].ToString();
				
				/* FOURTH */
				headstone.fields["FirstNameS_D_3"] = dataRow[(int)MasterTableCols.FirstNameS_D_3].ToString();
				headstone.fields["MiddleNameS_D_3"] = dataRow[(int)MasterTableCols.MiddleNameS_D_3].ToString();
				headstone.fields["LastNameS_D_3"] = dataRow[(int)MasterTableCols.LastNameS_D_3].ToString();
				headstone.fields["SuffixS_D_3"] = dataRow[(int)MasterTableCols.SuffixS_D_3].ToString();
				headstone.fields["LocationS_D_3"] = dataRow[(int)MasterTableCols.LocationS_D_3].ToString();
				headstone.fields["RankS_D_3"] = dataRow[(int)MasterTableCols.RankS_D_3].ToString();
				headstone.fields["AwardS_D_3"] = dataRow[(int)MasterTableCols.AwardS_D_3].ToString();
				headstone.fields["WarS_D_3"] = dataRow[(int)MasterTableCols.WarS_D_3].ToString();
				headstone.fields["BranchS_D_3"] = dataRow[(int)MasterTableCols.BranchS_D_3].ToString();
				headstone.fields["BirthDateS_D_3"] = dataRow[(int)MasterTableCols.BirthDateS_D_3].ToString();
				headstone.fields["DeathDateS_D_3"] = dataRow[(int)MasterTableCols.DeathDateS_D_3].ToString();
				headstone.fields["InscriptionS_D_3"] = dataRow[(int)MasterTableCols.InscriptionS_D_3].ToString();
				
				/* FIFTH */
				headstone.fields["FirstNameS_D_4"] = dataRow[(int)MasterTableCols.FirstNameS_D_4].ToString();
				headstone.fields["MiddleNameS_D_4"] = dataRow[(int)MasterTableCols.MiddleNameS_D_4].ToString();
				headstone.fields["LastNameS_D_4"] = dataRow[(int)MasterTableCols.LastNameS_D_4].ToString();
				headstone.fields["SuffixS_D_4"] = dataRow[(int)MasterTableCols.SuffixS_D_4].ToString();
				headstone.fields["LocationS_D_4"] = dataRow[(int)MasterTableCols.LocationS_D_4].ToString();
				headstone.fields["RankS_D_4"] = dataRow[(int)MasterTableCols.RankS_D_4].ToString();
				headstone.fields["AwardS_D_4"] = dataRow[(int)MasterTableCols.AwardS_D_4].ToString();
				headstone.fields["WarS_D_4"] = dataRow[(int)MasterTableCols.WarS_D_4].ToString();
				headstone.fields["BranchS_D_4"] = dataRow[(int)MasterTableCols.BranchS_D_4].ToString();
				headstone.fields["BirthDateS_D_4"] = dataRow[(int)MasterTableCols.BirthDateS_D_4].ToString();
				headstone.fields["DeathDateS_D_4"] = dataRow[(int)MasterTableCols.DeathDateS_D_4].ToString();
				headstone.fields["InscriptionS_D_4"] = dataRow[(int)MasterTableCols.InscriptionS_D_4].ToString();
				
				/* SIXTH */
				headstone.fields["FirstNameS_D_5"] = dataRow[(int)MasterTableCols.FirstNameS_D_5].ToString();
				headstone.fields["MiddleNameS_D_5"] = dataRow[(int)MasterTableCols.MiddleNameS_D_5].ToString();
				headstone.fields["LastNameS_D_5"] = dataRow[(int)MasterTableCols.LastNameS_D_5].ToString();
				headstone.fields["SuffixS_D_5"] = dataRow[(int)MasterTableCols.SuffixS_D_5].ToString();
				headstone.fields["LocationS_D_5"] = dataRow[(int)MasterTableCols.LocationS_D_5].ToString();
				headstone.fields["BirthDateS_D_5"] = dataRow[(int)MasterTableCols.BirthDateS_D_5].ToString();
				headstone.fields["DeathDateS_D_5"] = dataRow[(int)MasterTableCols.DeathDateS_D_5].ToString();
				
				/* SEVENTH */
				headstone.fields["FirstNameS_D_6"] = dataRow[(int)MasterTableCols.FirstNameS_D_6].ToString();
				headstone.fields["MiddleNameS_D_6"] = dataRow[(int)MasterTableCols.MiddleNameS_D_6].ToString();
				headstone.fields["LastNameS_D_6"] = dataRow[(int)MasterTableCols.LastNameS_D_6].ToString();
				headstone.fields["SuffixS_D_6"] = dataRow[(int)MasterTableCols.SuffixS_D_6].ToString();
				headstone.fields["LocationS_D_6"] = dataRow[(int)MasterTableCols.LocationS_D_6].ToString();
				headstone.fields["BirthDateS_D_6"] = dataRow[(int)MasterTableCols.BirthDateS_D_6].ToString();
				headstone.fields["DeathDateS_D_6"] = dataRow[(int)MasterTableCols.DeathDateS_D_6].ToString();
			
			
                Console.WriteLine((int)MasterTableCols.FrontFilename);
                headstone.fields["Image1FilePath"] = dataRow[(int)MasterTableCols.FrontFilename].ToString();
                headstone.fields["Image2FilePath"] = dataRow[(int)MasterTableCols.BackFilename].ToString();

                headstone.fields["Image1FileName"] = Path.GetFileName(headstone.fields["Image1FilePath"]);
                headstone.fields["Image2FileName"] = Path.GetFileName(headstone.fields["Image2FilePath"]);
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
            string sqlQuery = @"UPDATE Master SET ";

            // Append all keys and values to the string
            foreach (KeyValuePair<string, string> entry in headstone.fields)
            {
                sqlQuery += @"[" + entry.Key + @"] = " + @"@" + entry.Key + @", ";
            }

            /* May be unneccessary now, need testing
             * sqlQuery += "[Branch-Unit_CustomV] = '" + headstone.PrimaryDecedent.BranchUnitCustom +
                "', [Branch-Unit_CustomS_D] = '" + headstone.OthersDecedentList[0].BranchUnitCustom + "'";
            */

            // finalize update statement
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
                foreach (KeyValuePair<string, string> entry in headstone.fields)
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
    }
}
