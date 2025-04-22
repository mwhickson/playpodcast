using Terminal.Gui;

namespace playpodcast;

internal static class Program
{
    private static void Main()
    {
        Application.Init();
        Application.Run<MainWindow>();
        Application.Shutdown();
    }

}