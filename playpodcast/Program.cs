using Terminal.Gui;

internal class Program
{
    private static void Main(string[] args)
    {
        Application.Init();

        Application.Run<MainWindow>();

        // string url = "https://s.gum.fm/s-64df757f86d495f3f8ac7af4/traffic.megaphone.fm/BDA8396986280.mp3?updated=1743515791";

        // PodcastEpisodePlayer player = new(url);
        // player.Play();

        Application.Shutdown();

        // player.Stop();
    }

}
