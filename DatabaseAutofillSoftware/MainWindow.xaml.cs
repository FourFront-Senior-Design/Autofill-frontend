using System;
using ViewModelInterfaces;
using System.Diagnostics;
using System.Windows;
using ServicesInterface;
using DataStructures;

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


            // Autofill marker type
            Headstone currentHeadstone;
            for(int i = 1; i <= countData; i++)
            {
                bool updateDB = false;
                currentHeadstone = _database.GetHeadstone(i);
                // check if 2nd image exists in database
                if (currentHeadstone.Image2FileName == "")
                {
                    // Flat markers
                    if (currentHeadstone.MarkerType == "")
                    {
                        updateDB = true;
                        currentHeadstone.MarkerType = "Flat Marker";
                    }
                }
                else
                {
                    // Upright markers
                    if (currentHeadstone.MarkerType == "")
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


            // call ms access interface and push data to database
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
    }
}
