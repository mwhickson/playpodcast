using Terminal.Gui;

internal class Program
{
    private static void Main(string[] args)
    {
        Application.Init();
        Application.Run<MainWindow>();
        Application.Shutdown();
    }

}
