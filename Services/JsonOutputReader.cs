using DataStructures;
using ServicesInterface;
using Common;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System;

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
                currentHeadstone = _database.GetHeadstone(i);
                Dictionary<string, string> tmpData;

                // check if 2nd image exists in database
                if (string.IsNullOrWhiteSpace(currentHeadstone.fields["Image2FileName"]))
                {
                    // Flat markers
                    totalFlats++;
                    currentHeadstone.fields["MarkerType"] = "Flat Marker";
                }
                else
                {
                    // Upright headstones
                    totalUprights++;
                    currentHeadstone.fields["MarkerType"] = "Upright Headstone";
                }
                // Read data from file with the first images filename which contains all record data
                tmpData = ReadTmpFile(currentHeadstone.fields["Image1FileName"]);

                // Set the local headstone's data
                UpdateHeadstone(ref currentHeadstone, tmpData);

                // Update the database
                _database.SetHeadstone(i, currentHeadstone);

                foreach (string key in currentHeadstone.fields.Keys)
                {
                    if (!tmpData.ContainsKey(key))
                    {
                        Trace.WriteLine(key);
                    }
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

        private void UpdateHeadstone(ref Headstone currentHeadstone, Dictionary<string, string> tempData)
        {
            foreach (string key in tempData.Keys)
            {
                // Handle the odd cases of Branch Unit Custom
                if (key == "Branch-Unit_CustomV")
                    currentHeadstone.branchCustom = tempData[key];
                else if (key == "Branch-Unit_CustomS_D")
                    currentHeadstone.branchCustomS_D = tempData[key];
                else if (String.IsNullOrEmpty(currentHeadstone.fields[key]))
                {
                    currentHeadstone.fields[key] = tempData[key];
                }
            }
        }

        private Dictionary<string, string> ReadTmpFile(string filename)
        {
            // Private internal function to read file into Dictionary
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Encoding encoding = System.Text.Encoding.UTF8;
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
    }
}
