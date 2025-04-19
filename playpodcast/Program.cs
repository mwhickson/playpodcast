internal class Program
{
    private static void Main(string[] args)
    {
        UserInterface.DrawScreen();

        string url = "https://s.gum.fm/s-64df757f86d495f3f8ac7af4/traffic.megaphone.fm/BDA8396986280.mp3?updated=1743515791";

        PodcastEpisodePlayer player = new(url);
        player.Play();

        UserInterface.Prompt("Press <ENTER> to stop playback.");

        player.Stop();
    }

    class UserInterface
    {
        public static void DrawScreen()
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.Clear();

            Console.WriteLine("playaudio");

            // Console.BackgroundColor
            // Console.BufferHeight
            // Console.BufferWidth
            // Console.Clear
            // Console.CursorVisible
            // Console.ForegroundColor
            // Console.SetCursorPosition
            // Console.Title
            // Console.WindowHeight
            // Console.WindowWidth
        }

        public static void Prompt(string message)
        {
            Console.Write(message);
            Console.ReadLine();
        }
    }

}
