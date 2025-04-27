using LibVLCSharp.Shared;

namespace playpodcast;

public class Player
{
    private const bool DebugLoggingEnabled = false;

    private const string SecurityDialogTitle = "Insecure site";
    private const string PrimarySecurityActionText = "View certificate";
    private const string SecondarySecurityActionText = "Accept 24 hours";

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
        PlayPause();
    }

    public void PlayPause()
    {
        if (_Player != null)
        {
            if (_Player.IsPlaying)
            {
                _Player.Pause();
            }
            else
            {
                _Player.Play();
            }
        }
        else
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
    }

    public void Stop()
    {
        _Player?.Stop();
        _Player?.Dispose();
        _Audio?.Dispose();
    }

    private void CleanUp()
    {
        Stop();
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
        if (title != SecurityDialogTitle || !token.CanBeCanceled) return Task.CompletedTask;
        if (firstActionText == null) return Task.CompletedTask;

        switch (firstActionText)
        {
            case PrimarySecurityActionText:
            case SecondarySecurityActionText:
                dialog.PostAction(1);
                break;
        }

        return Task.CompletedTask;
    }

    // TODO: find out if this should this fire during playback?
    private static Task HandleDisplayProgressDialog(Dialog dialog, string? title, string? text, bool indeterminate, float position, string? cancelText, CancellationToken token)
    {
        // Console.WriteLine("PROGRESS: {0} - {1}", indeterminate ? "?" : "*", position.ToString());
        return Task.CompletedTask;
    }

    // TODO: find out if this should this fire during playback?
    private static Task HandleUpdateProgressDialog(Dialog dialog, float position, string? text)
    {
        // Console.WriteLine("PROGRESS-UPDATE: {0}", position.ToString());
        return Task.CompletedTask;
    }
}
