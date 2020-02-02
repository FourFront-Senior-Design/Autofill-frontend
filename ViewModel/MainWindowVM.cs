using ServicesInterface;
using System.ComponentModel;
using ViewModelInterfaces;

namespace ViewModel
{
    public class MainWindowVM: IMainWindowVM, INotifyPropertyChanged
    {
        public IDatabaseService _database;
        private string _fileLocation = string.Empty;
        private string _message;

        public string Copyright
        {
            get
            {
                return "Senior Design Data Extraction Project" + "\u00a9" + "2020. Version 1.0";
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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowVM(IDatabaseService database)
        {
            _database = database;
        }

        public int LoadData()
        {
            if (_database.InitDBConnection(_fileLocation) == false)
                return -1;
            return _database.TotalItems;
        }
    }
}
