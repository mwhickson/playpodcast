using LibVLCSharp.Shared;

namespace playpodcast;

public class Player
{
    private const bool DebugLoggingEnabled = false;

    private static LibVLC _vlc = new(DebugLoggingEnabled);
    private static string _FileLocation = "";
    private static Media? _Audio;
    private static MediaPlayer? _Player;

    public Player(string fileLocation = "")
    {
        _FileLocation = fileLocation;

        _vlc.SetDialogHandlers(
            HandleErrorDialog,
            HandleLoginDialog,
            HandleQuestionDialog,
            HandleDisplayProgressDialog,
            HandleUpdateProgressDialog
        );
    }

    public void PlayFileLocation(string? fileLocation = null)
    {
        _FileLocation = fileLocation ?? _FileLocation;
        Play();
    }

    public void Play()
    {
        if (!string.IsNullOrWhiteSpace(_FileLocation))
        {
            Uri url = new(_FileLocation);

            if (url.IsWellFormedOriginalString())
            {
                _Audio = new(_vlc, url);
                _Player = new(_Audio);
                _Player.Play();
            }
        }
    }

    public void Stop()
    {
        _Player?.Stop();
    }

    private void CleanUp()
    {
        Stop();

        _Player?.Dispose();
        _Audio?.Dispose();
        _vlc.Dispose();
    }

    //
    // VLC support functions
    //

    private static Task HandleErrorDialog(string? title, string? text)
    {
        // Console.WriteLine("VLC-ERROR: {0}", text);
        return Task.CompletedTask;
    }

    private static Task HandleLoginDialog(Dialog dialog, string? title, string? text, string? defaultUsername, bool askStore, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private static Task HandleQuestionDialog(Dialog dialog, string? title, string? text, DialogQuestionType questionType, string? cancelText, string? firstActionText, string? secondActionText, CancellationToken token)
    {
        // DEBUG:
        // Console.WriteLine();
        // Console.WriteLine("TITLE: {0}", title);
        // Console.WriteLine("TEXT: {0}", text);
        // Console.WriteLine("CANCEL: {0}", cancelText);
        // Console.WriteLine("ACTION 1: {0}", firstActionText);
        // Console.WriteLine("ACTION 2: {0}", secondActionText);
        // Console.WriteLine();

        //
        // TODO: fix raw text checks
        // (could just cheat and call PostAction(1) blindly, but would prefer clarity of code and future proofing...)
        //

        if (title is not "Insecure site" || !token.CanBeCanceled) return Task.CompletedTask;
        if (firstActionText == null) return Task.CompletedTask;

        switch (firstActionText)
        {
            case "View certificate":
            case "Accept 24 hours":
                dialog.PostAction(1);
                break;
        }

        return Task.CompletedTask;
    }

    private static Task HandleDisplayProgressDialog(Dialog dialog, string? title, string? text, bool indeterminate, float position, string? cancelText, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    private static Task HandleUpdateProgressDialog(Dialog dialog, float position, string? text)
    {
        return Task.CompletedTask;
    }
}
