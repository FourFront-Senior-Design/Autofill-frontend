namespace ViewModelInterfaces
{
    public interface IMainWindowVM
    {
        string FileLocation { get; set; }
        string Message { get; set; }
        string Copyright { get; }
        int LoadData();
    }
}
