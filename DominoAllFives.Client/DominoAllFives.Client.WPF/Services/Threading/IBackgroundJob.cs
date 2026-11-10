namespace DominoAllFives.Client.WPF.Services.Threading
{
    internal interface IBackgroundJob
    {
        void Execute();
        void Complete();
    }
}
