using LibVLCSharp.Shared;

namespace playpodcast;

public class PodcastEpisodePlayer
{
	private string? EpisodeUrl { get; }

	private const bool DebugLogging = false;
	private readonly LibVLC _vlc = new(DebugLogging);

	private Media? _audio;
	private MediaPlayer? _player;

	public PodcastEpisodePlayer(string? episodeUrl)
	{
		_vlc.SetDialogHandlers(
			VlcDisplayError,
			VlcDisplayLogin,
			VlcDisplayQuestion,
			VlcDisplayProgress,
			VlcUpdateProgress
		);

		EpisodeUrl = episodeUrl;
	}

	public void Play()
	{
		if (!string.IsNullOrWhiteSpace(EpisodeUrl))
		{
			_audio = new Media(_vlc, new Uri(EpisodeUrl));
			_player = new MediaPlayer(_audio);
			_player.Play();
		}
		else
		{
			throw new Exception("ERROR: Cannot start playback. No episode specified.");
		}
	}

	public void Stop()
	{
		if (_player != null)
		{
			_player.Stop();
			_player.Dispose();
		}

		_audio?.Dispose();
		_vlc.Dispose();
	}

	private static Task VlcDisplayError(string? title, string? text)
	{
		// Console.WriteLine("VLC-ERROR: {0}", text);
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

	private static Task VlcDisplayProgress(Dialog dialog, string? title, string? text, bool indeterminate, float position, string? cancelText, CancellationToken token)
	{
		return Task.CompletedTask;
	}

	private static Task VlcUpdateProgress(Dialog dialog, float position, string? text)
	{
		return Task.CompletedTask;
	}
}