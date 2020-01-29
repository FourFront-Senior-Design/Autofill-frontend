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

        private void OCRClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Printing...");
            Trace.WriteLine(_viewModel.FileLocation);

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

                _ocrService.extractText(_viewModel.FileLocation);

                _viewModel.Message = "Successfully processed " + countData.ToString() +
                                 " records.";
                _viewModel.EnableRun = true;
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


            // Autofill marker type and dates
            Headstone currentHeadstone;
            for(int i = 1; i <= countData; i++)
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
                    Dictionary<string, string> tmpData = ReadTmpFile(currentHeadstone);
                    updateDB = UpdateHeadstone(ref currentHeadstone, tmpData);

                }
                else
                {
                    // Upright markers
                    if (string.IsNullOrWhiteSpace(currentHeadstone.MarkerType))
                    {
                        updateDB = true;
                        currentHeadstone.MarkerType = "Upright Headstone";
                    }
                }
                if (updateDB)
                {
                    _database.SetHeadstone(i, currentHeadstone);
                }
                Trace.WriteLine("Record " + i + " processed.");
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

        private Dictionary<string, string> ReadTmpFile(Headstone record)
        {
            // Private internal function to read file into Dictionary
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Encoding encoding = System.Text.Encoding.UTF8;
            string result;

            // Set up filename
            string path = _viewModel.FileLocation + "\\tempFiles\\"
                + record.Image1FileName;
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

        private bool UpdateHeadstone(ref Headstone h, Dictionary<string, string> tmpData)
        {
            // Write dates to the Headstone - no overwrite of existing data
            // NOTE: Thise code needs to be refactored for multiple reasons:
            // 1) The access to non-primary decedents is terrible
            // 2) Convert from multiple if statements to a loop if its possbile
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

    }
}
