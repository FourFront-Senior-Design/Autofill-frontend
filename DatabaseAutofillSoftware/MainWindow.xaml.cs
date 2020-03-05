using ViewModelInterfaces;
using System.Windows;
using ServicesInterface;
using System.Collections.Generic;

namespace DatabaseAutofillSoftware
{
    public partial class MainWindow : Window
    {
        IMainWindowVM _viewModel;
        IOutputReader _outputReader;
        IAutofillController _autofillService;
        IDatabaseService _database;

        public MainWindow(IMainWindowVM viewModel, IDatabaseService database, 
            IAutofillController autofillController, IOutputReader outputReader)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _autofillService = autofillController;
            _database = database;
            _outputReader = outputReader;

            sectionPath.Focus();
            sectionPath.Select(_viewModel.FileLocation.Length, 0);
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

            sectionPath.Focus();
            sectionPath.Select(_viewModel.FileLocation.Length, 0);
        }

        private void AutofillClick(object sender, RoutedEventArgs e)
        {
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

                _viewModel.Message = "Database loaded successfully. Autofill scripts are running...";
                _database.CreateRecordTypeFile();
                _autofillService.runScripts(_viewModel.FileLocation);
                int missedRecordsCount = _outputReader.FillDatabase();

                if (missedRecordsCount > 0)
                {
                    _viewModel.Message = "Missed " + missedRecordsCount + " records, please retry this section.";
                }
                else
                {
                    _viewModel.Message = "Database autofilled successfully.";
                }
            }

            sectionPath.Focus();
            sectionPath.Select(_viewModel.FileLocation.Length, 0);
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Message = "";
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }
    }
}
