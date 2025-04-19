using LibVLCSharp.Shared;

class PodcastEpisodePlayer
{
	public string EpisodeUrl { get; set; } = "";

	const bool DEBUG_LOGGING = false;
	private readonly LibVLC vlc = new(DEBUG_LOGGING, []);

	private Media? audio;
	private MediaPlayer? player;

	private PodcastEpisodePlayer() { }

	public PodcastEpisodePlayer(string episodeUrl)
	{
		this.vlc.SetDialogHandlers(VlcDisplayError, VlcDisplayLogin, VlcDisplayQuestion, VlcDisplayProgress, VlcUpdateProgress);
		this.EpisodeUrl = episodeUrl;
	}

	public void Play()
	{
		if (!string.IsNullOrWhiteSpace(this.EpisodeUrl))
		{
			this.audio = new(vlc, new Uri(this.EpisodeUrl));
			this.player = new(audio);

			Console.WriteLine("playing {0}...", this.EpisodeUrl);

			this.player.Play();
		}
		else
		{
			Console.WriteLine("ERROR: Cannot start playback. No episode specified.");
		}
	}

	public void Stop()
	{
		if (this.player != null)
		{
			this.player.Stop();
			this.player.Dispose();
		}

		Console.WriteLine("Playback stopped.");

		this.audio?.Dispose();

		this.vlc.Dispose();
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

		//
		// TODO: fix raw text checks
		// (could just cheat and call PostAction(1) blindly, but would prefer clarity of code and future proofing...)
		//

		if (title != null && title == "Insecure site" && token.CanBeCanceled)
		{
			if (firstActionText != null)
			{
				switch (firstActionText)
				{
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
