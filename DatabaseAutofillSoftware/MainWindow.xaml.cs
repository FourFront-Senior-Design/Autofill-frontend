using System;
using ViewModelInterfaces;
using System.Diagnostics;
using System.Windows;
using ServicesInterface;
using DataStructures;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace DatabaseAutofillSoftware
{
    public partial class MainWindow : Window
    {
        IMainWindowVM _viewModel;
        IOCRService _ocrService;
        IAutofillController _autofillService;
        IDatabaseService _database;
        private bool _DeleteTempFilesDirectory = true;

        public MainWindow(IMainWindowVM viewModel, IDatabaseService database, IOCRService GoogleVision, IAutofillController autofillController)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _ocrService = GoogleVision;
            _autofillService = autofillController;
            _database = database;
            
        }
        
        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Message = "";
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            string selectedPath = dialog.SelectedPath;
            if (selectedPath != string.Empty)
            {
                _viewModel.FileLocation = selectedPath;
            }
        }

        private void AutofillClick(object sender, RoutedEventArgs e)
        {
            // Setup
            _autofillService.runScripts(_viewModel.FileLocation);

            int countData = _viewModel.LoadData();
            if (countData == -1)
            {
                _viewModel.Message = "Invalid Path. Try Again.";
            }
            else if (countData == 0)
            {
                _viewModel.Message = "No database found. Try Again.";
            }
            else
            {
                Properties.Settings.Default.databaseFilePath = _viewModel.FileLocation;
                Properties.Settings.Default.Save();
                _viewModel.Message = "Database loaded successfully.";
                _viewModel.EnableRun = true;
            }

            // Set up AllKeys nested list
            List<List<string>> AllKeys = new List<List<string>>();
            AllKeys.Add(EmptyList);
            AllKeys.Add(PrimaryKeys);
            AllKeys.Add(SecondKeys);
            AllKeys.Add(ThirdKeys);
            AllKeys.Add(FourthKeys);
            AllKeys.Add(FifthKeys);
            AllKeys.Add(SixthKeys);
            AllKeys.Add(SeventhKeys);

            // Autofill marker type and other data from .tmp files
            Headstone currentHeadstone;
            for (int i = 1; i <= countData; i++)
            {
                bool updateDB = false;
                currentHeadstone = _database.GetHeadstone(i);
                // check if 2nd image exists in database
                if (string.IsNullOrWhiteSpace(currentHeadstone.Image2FileName))
                {
                    // Flat markers
                    if (string.IsNullOrWhiteSpace(currentHeadstone.MarkerType))
                    {
                        updateDB = true;
                        currentHeadstone.MarkerType = "Flat Marker";
                    }

                    // Read single .tmp file for flat markers
                    Dictionary<string, string> tmpData = ReadTmpFile(currentHeadstone.Image1FileName);
                    updateDB = UpdateHeadstone(ref currentHeadstone, tmpData);
                }
                else
                {
                    // Upright headstones
                    if (string.IsNullOrWhiteSpace(currentHeadstone.MarkerType))
                    {
                        updateDB = true;
                        currentHeadstone.MarkerType = "Upright Headstone";
                    }

                    // Read both .tmp files for upright headstones
                    Dictionary<string, string> front = ReadTmpFile(currentHeadstone.Image1FileName);
                    Dictionary<string, string> back = ReadTmpFile(currentHeadstone.Image2FileName);
                    int numToMerge = 0;
                    int maxTotalDecedents = 7;

                    // Find last filled decedent position on front of stone
                    int lastFilledFront = FindLastFilledPosition(front, maxTotalDecedents);
                    // Find last filled decedent position on back of stone
                    int lastFilledBack = FindLastFilledPosition(back, maxTotalDecedents);

                    // Calculate the number of decedents to merge from back to front
                    if(lastFilledBack + lastFilledFront > maxTotalDecedents)
                    {
                        numToMerge = maxTotalDecedents - lastFilledFront;
                    }
                    else
                    {
                        numToMerge = lastFilledBack;
                    }

                    // TODO(jd): Put this into a separate function
                    int nextPosition = lastFilledFront + 1;
                    while (numToMerge > 0)
                    {
                        // need to merge into front starting at nextPosition
                        int backPosition = 1;
                        foreach(KeyValuePair<string, string> item in back)
                        {
                            // Only move keys at current backPosition
                            // TODO(jd): check that key also exists in nextPosition on front
                            if(AllKeys[backPosition].Contains(item.Key))
                            {
                                // Verify key has a value
                                // TODO(jd): Currently only dates are handled
                                // TODO(jd): Need to handle more fields here
                                if(!string.IsNullOrWhiteSpace(item.Value))
                                {
                                    if (BirthDateKeys.Contains(item.Key))
                                    {
                                        front[BirthDateKeys[nextPosition]] = item.Value;
                                    }
                                    if (DeathDateKeys.Contains(item.Key))
                                    {
                                        front[DeathDateKeys[nextPosition]] = item.Value;
                                    }
                                }
                            }
                        }
                        nextPosition++;
                        backPosition++;
                        numToMerge--;
                    }

                    // update headstone with combined data
                    updateDB = UpdateHeadstone(ref currentHeadstone, front);

                    if (updateDB)
                    {
                        _database.SetHeadstone(i, currentHeadstone);
                    }
                    //Trace.WriteLine("Record " + i + " processed.");
                }
            }
            // delete tempFiles directory
            if (_DeleteTempFilesDirectory)
            {
                string tempFilesPath = _viewModel.FileLocation + "\\tempFiles\\";
                System.IO.Directory.Delete(tempFilesPath, true);
            }
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.EnableRun = false;
            _viewModel.Message = "";
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private Dictionary<string, string> ReadTmpFile(string filename)
        {
            // Private internal function to read file into Dictionary
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Encoding encoding = System.Text.Encoding.UTF8;
            string result;

            // Set up filename
            string path = _viewModel.FileLocation + "\\tempFiles\\"
                + filename;
            // Replace .jpg extension from file name
            path = path.Remove(path.Length-4, 4);
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
                    dict.Add(line[0], line[1]);
                }
                else if (line.Length == 1 && !string.IsNullOrEmpty(line[0]))
                {
                    dict.Add(line[0], "");
                }
            }

            return dict;
        }


        private int FindLastFilledPosition(Dictionary<string, string> dict , int maxTotalDecedents)
        {
            int lastFilledPosition = 0;
            foreach (KeyValuePair<string, string> item in dict)
            {
                if (!string.IsNullOrWhiteSpace(item.Value))
                {
                    if (PrimaryKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 1;
                    }
                    else if (SecondKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 2;
                    }
                    else if (ThirdKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 3;
                    }
                    else if (FourthKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 4;
                    }
                    else if (FifthKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 5;
                    }
                    else if (SixthKeys.Contains(item.Key))
                    {
                        lastFilledPosition = 6;
                    }
                    else if (SeventhKeys.Contains(item.Key))
                    {
                        lastFilledPosition = maxTotalDecedents;
                    }
                }
            }
            return lastFilledPosition;
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

            if(tmpData.TryGetValue("First Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.FirstName))
            {
                h.PrimaryDecedent.FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Middle Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.MiddleName))
            {
                h.PrimaryDecedent.MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Last Name", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.LastName))
            {
                h.PrimaryDecedent.LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Suffix", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Suffix))
            {
                h.PrimaryDecedent.Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Location", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Location))
            {
                h.PrimaryDecedent.Location= value;
                updateDB = true;
            }
            List<string> Ranks = new List<string>() { "Rank", "Rank2", "Rank3" };
            for(int i = 0; i < 3; i++)
            {
                if(tmpData.TryGetValue(Ranks[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.RankList[i]))
                {
                    h.PrimaryDecedent.RankList[i] = value;
                    updateDB = true;
                }
            }
            List<string> Branches = new List<string>() { "Branch", "Branch2", "Branch3" };
            for(int i = 0; i < 3; i++)
            {
                if(tmpData.TryGetValue(Branches[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BranchList[i]))
                {
                    h.PrimaryDecedent.BranchList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("Branch-Unit_CustomV", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BranchUnitCustom))
            {
                h.PrimaryDecedent.BranchUnitCustom = value;
                updateDB = true;
            }
            List<string> Wars = new List<string>() { "War", "War2", "War3", "War4" };
            for(int i = 0; i < 4; i++)
            {
                if(tmpData.TryGetValue(Wars[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.WarList[i]))
                {
                    h.PrimaryDecedent.WarList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("BirthDate", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.BirthDate))
            {
                h.PrimaryDecedent.BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDate", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.DeathDate))
            {
                h.PrimaryDecedent.DeathDate = value;
                updateDB = true;
            }
            List<string> Awards = new List<string>() { "Award", "Award2", "Award3", "Award4", "Award5", "Award6", "Award7" };
            for(int i = 0; i < 7; i++)
            {
                if(tmpData.TryGetValue(Awards[i], out value)
                    && string.IsNullOrWhiteSpace(h.PrimaryDecedent.AwardList[i]))
                {
                    h.PrimaryDecedent.AwardList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("Awards_Custom", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.AwardCustom))
            {
                h.PrimaryDecedent.AwardCustom = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Inscription", out value)
                && string.IsNullOrWhiteSpace(h.PrimaryDecedent.Inscription))
            {
                h.PrimaryDecedent.Inscription = value;
                updateDB = true;
            }
            // Secondary 
            if(tmpData.TryGetValue("First Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].FirstName))
            {
                h.OthersDecedentList[0].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Middle Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].MiddleName))
            {
                h.OthersDecedentList[0].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Last Name Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].LastName))
            {
                h.OthersDecedentList[0].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("Suffix Spouse/Dependent", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Suffix))
            {
                h.OthersDecedentList[0].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Location))
            {
                h.OthersDecedentList[0].Location = value;
                updateDB = true;
            }
            List<string> RankSD = new List<string>() { "RankS_D", "Rank2S_D", "Rank3S_D" };
            for(int i = 0; i < 3; i++)
            {
                if(tmpData.TryGetValue(RankSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].RankList[i]))
                {
                    h.OthersDecedentList[0].RankList[i] = value;
                    updateDB = true;
                }
            }
            List<string> BranchSD = new List<string>() { "Branch", "Branch2", "Branch3" };
            for(int i = 0; i < 3; i++)
            {
                if(tmpData.TryGetValue(BranchSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BranchList[i]))
                {
                    h.OthersDecedentList[0].BranchList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("Branch-Unit_CustomS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BranchUnitCustom))
            {
                h.OthersDecedentList[0].BranchUnitCustom = value;
                updateDB = true;
            }
            List<string> WarSD = new List<string>() { "WarS_D", "War2S_D", "War3S_D", "War4S_D" };
            for(int i = 0; i < 4; i++)
            {
                if(tmpData.TryGetValue(WarSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].WarList[i]))
                {
                    h.OthersDecedentList[0].WarList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("BirthDateS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].BirthDate))
            {
                h.OthersDecedentList[0].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].DeathDate))
            {
                h.OthersDecedentList[0].DeathDate = value;
                updateDB = true;
            }
            List<string> AwardSD = new List<string>() { "AwardS_D", "Award2S_D", "Award3S_D", "Award4S_D", "Award5S_D", "Award6S_D", "Award7S_D" };
            for(int i = 0; i < 7; i++)
            {
                if(tmpData.TryGetValue(AwardSD[i], out value)
                    && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].AwardList[i]))
                {
                    h.OthersDecedentList[0].AwardList[i] = value;
                    updateDB = true;
                }
            }
            if(tmpData.TryGetValue("Awards_CustomS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].AwardCustom))
            {
                h.OthersDecedentList[0].AwardCustom = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("InscriptionS_D", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[0].Inscription))
            {
                h.OthersDecedentList[0].Inscription = value;
                updateDB = true;
            }
            // Third
            if(tmpData.TryGetValue("FirstNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].FirstName))
            {
                h.OthersDecedentList[1].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("MiddleNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].MiddleName))
            {
                h.OthersDecedentList[1].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LastNameS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].LastName))
            {
                h.OthersDecedentList[1].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("SuffixS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Suffix))
            {
                h.OthersDecedentList[1].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Location))
            {
                h.OthersDecedentList[1].Location = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("RankS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].RankList[0]))
            {
                h.OthersDecedentList[1].RankList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BranchS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].BranchList[0]))
            {
                h.OthersDecedentList[1].BranchList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("WarS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].WarList[0]))
            {
                h.OthersDecedentList[1].WarList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BirthDateS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].BirthDate))
            {
                h.OthersDecedentList[1].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].DeathDate))
            {
                h.OthersDecedentList[1].DeathDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("AwardS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].AwardList[0]))
            {
                h.OthersDecedentList[1].AwardList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("InscriptionS_D_2", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[1].Inscription))
            {
                h.OthersDecedentList[1].Inscription = value;
                updateDB = true;
            }
            // Fourth
            if(tmpData.TryGetValue("FirstNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].FirstName))
            {
                h.OthersDecedentList[2].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("MiddleNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].MiddleName))
            {
                h.OthersDecedentList[2].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LastNameS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].LastName))
            {
                h.OthersDecedentList[2].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("SuffixS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Suffix))
            {
                h.OthersDecedentList[2].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Location))
            {
                h.OthersDecedentList[2].Location = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("RankS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].RankList[0]))
            {
                h.OthersDecedentList[2].RankList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BranchS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].BranchList[0]))
            {
                h.OthersDecedentList[2].BranchList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("WarS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].WarList[0]))
            {
                h.OthersDecedentList[2].WarList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BirthDateS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].BirthDate))
            {
                h.OthersDecedentList[2].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].DeathDate))
            {
                h.OthersDecedentList[2].DeathDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("AwardS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].AwardList[0]))
            {
                h.OthersDecedentList[2].AwardList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("InscriptionS_D_3", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[2].Inscription))
            {
                h.OthersDecedentList[2].Inscription = value;
                updateDB = true;
            }
            // Fifth
            if(tmpData.TryGetValue("FirstNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].FirstName))
            {
                h.OthersDecedentList[3].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("MiddleNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].MiddleName))
            {
                h.OthersDecedentList[3].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LastNameS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].LastName))
            {
                h.OthersDecedentList[3].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("SuffixS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Suffix))
            {
                h.OthersDecedentList[3].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Location))
            {
                h.OthersDecedentList[3].Location = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("RankS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].RankList[0]))
            {
                h.OthersDecedentList[3].RankList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BranchS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].BranchList[0]))
            {
                h.OthersDecedentList[3].BranchList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("WarS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].WarList[0]))
            {
                h.OthersDecedentList[3].WarList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BirthDateS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].BirthDate))
            {
                h.OthersDecedentList[3].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].DeathDate))
            {
                h.OthersDecedentList[3].DeathDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("AwardS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].AwardList[0]))
            {
                h.OthersDecedentList[3].AwardList[0] = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("InscriptionS_D_4", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[3].Inscription))
            {
                h.OthersDecedentList[3].Inscription = value;
                updateDB = true;
            }
            // Sixth
            if(tmpData.TryGetValue("FirstNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].FirstName))
            {
                h.OthersDecedentList[4].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("MiddleNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].MiddleName))
            {
                h.OthersDecedentList[4].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LastNameS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].LastName))
            {
                h.OthersDecedentList[4].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("SuffixS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].Suffix))
            {
                h.OthersDecedentList[4].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].Location))
            {
                h.OthersDecedentList[4].Location = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BirthDateS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].BirthDate))
            {
                h.OthersDecedentList[4].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D_5", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[4].DeathDate))
            {
                h.OthersDecedentList[4].DeathDate = value;
                updateDB = true;
            }
            // Seventh
            if(tmpData.TryGetValue("FirstNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].FirstName))
            {
                h.OthersDecedentList[5].FirstName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("MiddleNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].MiddleName))
            {
                h.OthersDecedentList[5].MiddleName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LastNameS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].LastName))
            {
                h.OthersDecedentList[5].LastName = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("SuffixS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].Suffix))
            {
                h.OthersDecedentList[5].Suffix = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("LocationS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].Location))
            {
                h.OthersDecedentList[5].Location = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("BirthDateS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].BirthDate))
            {
                h.OthersDecedentList[5].BirthDate = value;
                updateDB = true;
            }
            if(tmpData.TryGetValue("DeathDateS_D_6", out value)
                && string.IsNullOrWhiteSpace(h.OthersDecedentList[5].DeathDate))
            {
                h.OthersDecedentList[5].DeathDate = value;
                updateDB = true;
            }

            return updateDB; 
        }

        // Key Lists
        List<string> PrimaryKeys = new List<string>()
        {
            "First Name",
            "Middle Name",
            "Last Name",
            "Suffix",
            "Location",
            "Rank",
            "Rank2",
            "Rank3",
            "Branch",
            "Branch2",
            "Branch3",
            "Branch-Unit_CustomV",
            "War",
            "War2",
            "War3",
            "War4",
            "BirthDate",
            "DeathDate",
            "Award",
            "Award2",
            "Award3",
            "Award4",
            "Award5",
            "Award6",
            "Award7",
            "Awards_Custom",
            "Inscription"
        };

        List<string> SecondKeys = new List<string>()
        {
            "First Name Spouse/Dependent",
            "Middle Name Spouse/Dependent",
            "Last Name Spouse/Dependent",
            "Suffix Spouse/Dependent",
            "LocationS_D",
            "RankS_D",
            "Rank2S_D",
            "Rank3S_D",
            "BranchS_D",
            "Branch2S_D",
            "Branch3S_D",
            "Branch-Unit_CustomS_D",
            "WarS_D",
            "War2S_D",
            "War3S_D",
            "War4S_D",
            "BirthDateS_D",
            "DeathDateS_D",
            "AwardS_D",
            "Award2S_D",
            "Award3S_D",
            "Award4S_D",
            "Award5S_D",
            "Award6S_D",
            "Award7S_D",
            "Awards_CustomS_D",
            "InscriptionS_D"
        };

        List<string> ThirdKeys = new List<string>()
        {
            "FirstNameS_D_2",
            "MiddleNameS_D_2",
            "LastNameS_D_2",
            "SuffixS_D_2",
            "LocationS_D_2",
            "RankS_D_2",
            "BranchS_D_2",
            "WarS_D_2",
            "BirthDateS_D_2",
            "DeathDateS_D_2",
            "AwardS_D_2",
            "InscriptionS_D_2"
        };

        List<string> FourthKeys = new List<string>()
        {
            "FirstNameS_D_3",
            "MiddleNameS_D_3",
            "LastNameS_D_3",
            "SuffixS_D_3",
            "LocationS_D_3",
            "RankS_D_3",
            "BranchS_D_3",
            "WarS_D_3",
            "BirthDateS_D_3",
            "DeathDateS_D_3",
            "AwardS_D_3",
            "InscriptionS_D_3"
        };

        List<string> FifthKeys = new List<string>()
        {
            "FirstNameS_D_4",
            "MiddleNameS_D_4",
            "LastNameS_D_4",
            "SuffixS_D_4",
            "LocationS_D_4",
            "RankS_D_4",
            "BranchS_D_4",
            "WarS_D_4",
            "BirthDateS_D_4",
            "DeathDateS_D_4",
            "AwardS_D_4",
            "InscriptionS_D_4"
        };

        List<string> SixthKeys = new List<string>()
        {
            "FirstNameS_D_5",
            "MiddleNameS_D_5",
            "LastNameS_D_5",
            "SuffixS_D_5",
            "LocationS_D_5",
            "BirthDateS_D_5",
            "DeathDateS_D_5",
        };

        List<string> SeventhKeys = new List<string>()
        {
            "FirstNameS_D_6",
            "MiddleNameS_D_6",
            "LastNameS_D_6",
            "SuffixS_D_6",
            "LocationS_D_6",
            "BirthDateS_D_6",
            "DeathDateS_D_6",
        };

        List<string> EmptyList = new List<string>()
        {
            "placeholder"
        };


        List<string> FirstNameKeys = new List<string>()
        {
            "placeholder",
            "First Name",
            "First Name Spouse/Dependent",
            "FirstNameS_D_2",
            "FirstNameS_D_3",
            "FirstNameS_D_4",
            "FirstNameS_D_5",
            "FirstNameS_D_6"
        };

        List<string> MiddleNameKeys = new List<string>()
        {
            "placeholder",
            "Middle Name",
            "Middle Name Spouse/Dependent",
            "MiddleNameS_D_2",
            "MiddleNameS_D_3",
            "MiddleNameS_D_4",
            "MiddleNameS_D_5",
            "MiddleNameS_D_6"
        };

        List<string> LastNameKeys = new List<string>()
        {
            "placeholder",
            "Last Name",
            "Last Name Spouse/Dependent",
            "LastNameS_D_2",
            "LastNameS_D_3",
            "LastNameS_D_4",
            "LastNameS_D_5",
            "LastNameS_D_6"
        };

        List<string> SuffixKeys = new List<string>()
        {
            "placeholder",
            "Suffix",
            "Suffix Spouse/Dependent",
            "SuffixS_D_2",
            "SuffixS_D_3",
            "SuffixS_D_4",
            "SuffixS_D_5",
            "SuffixS_D_6"
        };

        List<string> LocationKeys = new List<string>()
        {
            "placeholder",
            "Location",
            "LocationS_D",
            "LocationS_D_2",
            "LocationS_D_3",
            "LocationS_D_4",
            "LocationS_D_5",
            "LocationS_D_6"
        };

        List<string> RankKeys = new List<string>()
        {
            "placeholder",
            "Rank",
            "RankS_D",
            "RankS_D_2",
            "RankS_D_3",
            "RankS_D_4",
        };

        List<string> Rank2Keys = new List<string>()
        {
            "Rank2",
            "Rank2S_D",
        };

        List<string> Rank3Keys = new List<string>()
        {
            "Rank3",
            "Rank3S_D",
        };

        List<string> BranchKeys = new List<string>()
        {
            "placeholder",
            "Branch",
            "BranchS_D",
            "BranchS_D_2",
            "BranchS_D_3",
            "BranchS_D_4",
        };

        List<string> Branch2Keys = new List<string>()
        {
            "Branch2",
            "Branch2S_D",
        };

        List<string> Branch3Keys = new List<string>()
        {
            "Branch3",
            "Branch3S_D",
        };

        List<string> BranchCustomKeys = new List<string>()
        {
            "Branch-Unit_CustomV",
            "Branch-Unit_CustomS_D"
        };

        List<string> WarKeys = new List<string>()
        {
            "placeholder",
            "War",
            "WarS_D",
            "WarS_D_2",
            "WarS_D_3",
            "WarS_D_4",
        };

        List<string> War2Keys = new List<string>()
        {
            "War2",
            "War2S_D",
        };

        List<string> War3Keys = new List<string>()
        {
            "War3",
            "War3S_D",
        };

        List<string> War4Keys = new List<string>()
        {
            "War4",
            "War4S_D",
        };

        List<string> BirthDateKeys = new List<string>()
        {
            "placeholder",
            "BirthDate",
            "BirthDateS_D",
            "BirthDateS_D_2",
            "BirthDateS_D_3",
            "BirthDateS_D_4",
            "BirthDateS_D_5",
            "BirthDateS_D_6"
        };
        
        List<string> DeathDateKeys = new List<string>()
        {
            "placeholder",
            "DeathDate",
            "DeathDateS_D",
            "DeathDateS_D_2",
            "DeathDateS_D_3",
            "DeathDateS_D_4",
            "DeathDateS_D_5",
            "DeathDateS_D_6"
        };

        List<string> AwardKeys = new List<string>()
        {
            "placeholder",
            "Award",
            "AwardS_D",
            "AwardS_D_2",
            "AwardS_D_3",
            "AwardS_D_4",
        };

        List<string> Award2Keys = new List<string>()
        {
            "placeholder",
            "Award2",
            "Award2S_D",
        };

        List<string> Award3Keys = new List<string>()
        {
            "placeholder",
            "Award3",
            "Award3S_D",
        };

        List<string> Award4Keys = new List<string>()
        {
            "placeholder",
            "Award4",
            "Award4S_D",
        };

        List<string> Award5Keys = new List<string>()
        {
            "placeholder",
            "Award5",
            "Award5S_D",
        };

        List<string> Award6Keys = new List<string>()
        {
            "placeholder",
            "Award6",
            "Award6S_D",
        };

        List<string> Award7Keys = new List<string>()
        {
            "placeholder",
            "Award7",
            "Award7S_D",
        };

        List<string> AwardCustomKeys = new List<string>()
        {
            "placeholder",
            "Awards_Custom",
            "Awards_CustomS_D",
        };

        List<string> InscriptionKeys = new List<string>()
        {
            "placeholder",
            "Inscription",
            "InscriptionS_D",
            "InscriptionS_D_2",
            "InscriptionS_D_3",
            "InscriptionS_D_4"
        };
    }
}
