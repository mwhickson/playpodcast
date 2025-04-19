using LibVLCSharp.Shared;

internal class Program
{
    private static void Main(string[] args)
    {
        const bool DEBUG_LOGGING = true;

        // const string url = "https://traffic.libsyn.com/secure/93e15648-e342-4262-a315-f51d3313778e/king-falls-am-update.mp3";
        const string url = "https://media.blubrry.com/codingblocks/www.podtrac.com/pts/redirect.mp3/traffic.libsyn.com/codingblocks/coding-blocks-episode-242.mp3";

        // const string url = "https://s.gum.fm/s-64df757f86d495f3f8ac7af4/traffic.megaphone.fm/BDA8396986280.mp3?updated=1743515791"; // works

        Console.WriteLine("playaudio");

        // LibVLC vlc = new(DEBUG_LOGGING, ["--no-gnutls-system-trust"]);
        // LibVLC vlc = new(DEBUG_LOGGING, ["--no-gnutls-system-trust", "--mp4-m4a-audioonly"]);
        LibVLC vlc = new(DEBUG_LOGGING, []);

        vlc.SetDialogHandlers(VlcDisplayError, VlcDisplayLogin, VlcDisplayQuestion, VlcDisplayProgress, VlcUpdateProgress);

        Media audio = new(vlc, new Uri(url));
        MediaPlayer player = new(audio);

        player.Play();

        Console.Write("playing...");
        Console.ReadLine();

        player.Dispose();
        audio.Dispose();
        vlc.Dispose();
    }

    private static Task VlcDisplayError(string? title, string? text)
    {
        Console.WriteLine("VLC-ERROR: {0}", text);
        return Task.CompletedTask;
    }

    private static Task VlcDisplayLogin(Dialog dialog, string? title, string? text, string? defaultUsername, bool askStore, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private static Task VlcDisplayQuestion(Dialog dialog, string? title, string? text, DialogQuestionType questionType, string? cancelText, string? firstActionText, string? secondActionText, CancellationToken token)
    {
        // DEBUG:
        // Console.WriteLine();
        // Console.WriteLine("TITLE: {0}", title);
        // Console.WriteLine("TEXT: {0}", text);
        // Console.WriteLine("CANCEL: {0}", cancelText);
        // Console.WriteLine("ACTION 1: {0}", firstActionText);
        // Console.WriteLine("ACTION 2: {0}", secondActionText);
        // Console.WriteLine();

        if (title != null && title == "Insecure site" && token.CanBeCanceled) {
            if (firstActionText != null) {
                switch(firstActionText) {
                    case "View certificate":
                        dialog.PostAction(1);
                        break;
                    case "Accept 24 hours":
                        dialog.PostAction(1);
                        break;
                    default:
                        // PASS:
                        break;
                }
            }
        }

        return Task.CompletedTask;
    }

    private static Task VlcDisplayProgress(Dialog dialog, string? title, string? text, bool indeterminate, float position, string? cancelText, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private static Task VlcUpdateProgress(Dialog dialog, float position, string? text)
    {
        return Task.CompletedTask;
    }

}