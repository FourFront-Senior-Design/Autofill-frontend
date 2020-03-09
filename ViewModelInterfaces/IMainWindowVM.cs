namespace ViewModelInterfaces
{
    public interface IMainWindowVM
    {
        string FileLocation { get; set; }
        string Message { get; set; }
        string Copyright { get; }
        string Title { get; }

        void SetFilePath(string path);
        void SetMessage(string message);
        bool LoadData();
    }
}
