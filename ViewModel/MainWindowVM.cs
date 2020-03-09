using ServicesInterface;
using System.ComponentModel;
using ViewModelInterfaces;

namespace ViewModel
{
    public class MainWindowVM: IMainWindowVM, INotifyPropertyChanged
    {
        public IDatabaseService _database;
        private string _fileLocation;
        private string _message;
        private string _version;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowVM(IDatabaseService database)
        {
            _database = database;
            _fileLocation = "";
            _message = "";
            _version = System.Reflection.AssemblyName.GetAssemblyName("DatabaseAutofillSoftware.exe").Version.ToString();
        }

        public string Copyright
        {
            get
            {
                string copyrightSymbol = "\u00a9";
                return $"Senior Design Data Extraction Project {copyrightSymbol} 2019. Version {_version}";
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        public string FileLocation
        {
            get
            {
                return _fileLocation;
            }
            set
            {
                _fileLocation = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileLocation)));
            }
        }

        public string Title
        {
            get
            {
                return $"Database Auto-fill Software (Version {_version})";
            }
        }

        public void SetFilePath(string selectedPath)
        {
            if (selectedPath != string.Empty)
            {
                Message = "";
                FileLocation = selectedPath;
            }
        }

        public void SetMessage(string message)
        {
            Message = message;
        }

        public bool LoadData()
        {
            if (_database.InitDBConnection(_fileLocation) == false)
            {
                Message = "Invalid Path. Try Again.";
                return false;
            }

            int count = _database.TotalItems;
            if (count == 0)
            {
                Message = "No records found in the Database. Try Again.";
                return false;
            }
            else
            {
                Message = "Successfully loaded " + count.ToString() + " records from the Database.";
            }
            return true;
        }
    }
}
