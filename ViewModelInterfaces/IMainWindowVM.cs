namespace ViewModelInterfaces
{
    public interface IMainWindowVM
    {
        bool EnableRun { get; set; }
        string FileLocation { get; set; }
        string Message { get; set; }
        string Copyright { get; }
        int LoadData();
    }
}
