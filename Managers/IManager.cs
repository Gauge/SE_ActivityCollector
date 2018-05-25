namespace ActivityCollectorPlugin.Managers
{
    public interface IManager
    {
        bool IsInitialized { get; }
        void Run();
    }
}
