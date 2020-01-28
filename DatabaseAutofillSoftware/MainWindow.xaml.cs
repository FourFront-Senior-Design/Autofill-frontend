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
                    // Read .tmp file for this record
                    string tmpPath = _viewModel.FileLocation + "\\tempFiles\\"
                        + currentHeadstone.Image1FileName;
                    // Replace .jpg extension from file name
                    tmpPath = tmpPath.Remove(tmpPath.Length-4, 4);
                    tmpPath += ".tmp";

                    // Read .tmp file into string - convert to list
                    String tmpString = ReadFile(tmpPath, System.Text.Encoding.UTF8);
                    List<string> tmpList = new List<string>(tmpString.Split('\n'));

                    // Set up dictionary of key,value pairs from file
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();
                    foreach (string item in tmpList)
                    {
                        // Add only key,value pairs that exist
                        string[] line = item.Split(':');
                        if (line.Length == 2)
                        {
                            tmpDict.Add(line[0], line[1]);
                        }
                    }

                    // Verify that the tmpDict has the correct data
                    //foreach(KeyValuePair<string, string> kv in tmpDict)
                    //{
                    //    Trace.WriteLine(kv);
                    //}

                    // Write dates directly to the Headstone
                    // NOTE: Thise code needs to be refactored for multiple reasons:
                    // 1) The access to non-primary decedents is terrible
                    // 2) Convert from multiple if statements to a loop
                    // 3) If possible, make this a generic "write Headstone to database" function
                    // Primary  
                    tmpDict.TryGetValue("BirthDate", out string bdate);
                    tmpDict.TryGetValue("DeathDate", out string ddate);
                    tmpDict.TryGetValue("BirthDateS_D", out string bdate2);
                    tmpDict.TryGetValue("DeathDateS_D", out string ddate2);
                    //Trace.WriteLine("Dates in tmpDict:");
                    Trace.WriteLine(currentHeadstone.PrimaryDecedent.LastName);
                    //Trace.WriteLine(bdate);
                    //Trace.WriteLine(ddate);
                    //Trace.WriteLine(bdate2);
                    //Trace.WriteLine(ddate2);
                    if(tmpDict.TryGetValue("BirthDate", out string birthDate1)
                        && string.IsNullOrWhiteSpace(currentHeadstone.PrimaryDecedent.BirthDate))
                    {
                        currentHeadstone.PrimaryDecedent.BirthDate = birthDate1;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDate", out string deathDate1)
                        && string.IsNullOrWhiteSpace(currentHeadstone.PrimaryDecedent.DeathDate))
                    {
                        currentHeadstone.PrimaryDecedent.DeathDate = deathDate1;
                        updateDB = true;
                    }
                    // Secondary
                    if(tmpDict.TryGetValue("BirthDateS_D", out string birthDate2)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[0].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[0].BirthDate = birthDate2;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D", out string deathDate2)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[0].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[0].DeathDate = deathDate2;
                        updateDB = true;
                    }
                    // Third (NOTE: the S_D_x is correct - it's off by one)
                    if(tmpDict.TryGetValue("BirthDateS_D_2", out string birthDate3)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[1].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[1].BirthDate = birthDate3;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D_2", out string deathDate3)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[1].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[1].DeathDate = deathDate3;
                        updateDB = true;
                    }
                    // Fourth (NOTE: the S_D_x is correct - it's off by one)
                    if(tmpDict.TryGetValue("BirthDateS_D_3", out string birthDate4)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[2].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[2].BirthDate = birthDate4;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D_3", out string deathDate4)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[2].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[2].DeathDate = deathDate4;
                        updateDB = true;
                    }
                    // Fifth (NOTE: the S_D_x is correct - it's off by one)
                    if(tmpDict.TryGetValue("BirthDateS_D_4", out string birthDate5)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[3].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[3].BirthDate = birthDate5;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D_4", out string deathDate5)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[3].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[3].DeathDate = deathDate5;
                        updateDB = true;
                    }
                    // Sixth (NOTE: the S_D_x is correct - it's off by one)
                    if(tmpDict.TryGetValue("BirthDateS_D_5", out string birthDate6)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[4].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[4].BirthDate = birthDate6;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D_5", out string deathDate6)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[4].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[4].DeathDate = deathDate6;
                        updateDB = true;
                    }
                    // Seventh (NOTE: the S_D_x is correct - it's off by one)
                    if(tmpDict.TryGetValue("BirthDateS_D_6", out string birthDate7)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[5].BirthDate))
                    {
                        currentHeadstone.OthersDecedentList[5].BirthDate = birthDate7;
                        updateDB = true;
                    }
                    if(tmpDict.TryGetValue("DeathDateS_D_6", out string deathDate7)
                        && string.IsNullOrWhiteSpace(currentHeadstone.OthersDecedentList[5].DeathDate))
                    {
                        currentHeadstone.OthersDecedentList[5].DeathDate = deathDate7;
                        updateDB = true;
                    }
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

        private static string ReadFile(string path, Encoding encoding)
        {
            // Private internal function to read file to string
            string result;
            using (StreamReader streamReader = new StreamReader(path, encoding))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }
    }
}
