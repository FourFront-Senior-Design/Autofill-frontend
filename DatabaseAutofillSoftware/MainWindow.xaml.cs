﻿using ViewModelInterfaces;
using System.Windows;
using System.ComponentModel;
using System.Threading;
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

            autofillProgress.Visibility = Visibility.Collapsed;
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.ShowDialog();

            string selectedPath = dialog.SelectedPath;
            _viewModel.SetFilePath(selectedPath);

            sectionPath.Focus();
            sectionPath.Select(_viewModel.FileLocation.Length, 0);
        }

        private void AutofillClick(object sender, RoutedEventArgs e)
        {
            if(_viewModel.LoadData())
            {
                autofillProgress.Visibility = Visibility.Visible;
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;

                worker.RunWorkerAsync();
            }

            sectionPath.Focus();
            sectionPath.Select(_viewModel.FileLocation.Length, 0);
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            (sender as BackgroundWorker).ReportProgress(1);

            Properties.Settings.Default.databaseFilePath = _viewModel.FileLocation;
            Properties.Settings.Default.Save();

            _viewModel.SetMessage("Database loaded successfully...");
            _database.CreateRecordTypeFile();

            (sender as BackgroundWorker).ReportProgress(10);
            
            _viewModel.SetMessage("Google Vision running...");

            _autofillService.runScripts(_viewModel.FileLocation);

            (sender as BackgroundWorker).ReportProgress(75);

            _viewModel.SetMessage("Autofill scripts running...");

            int missedRecordsCount = _outputReader.FillDatabase();

            (sender as BackgroundWorker).ReportProgress(100);

            if (missedRecordsCount > 0)
            {
                _viewModel.SetMessage("Missed " + missedRecordsCount + " records, please retry this section");
            }
            else
            {
                _viewModel.SetMessage("Database autofilled successfully");
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            autofillProgress.Value = e.ProgressPercentage;
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.SetMessage(string.Empty);
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }
    }
}
