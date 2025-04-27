namespace playpodcast;

internal static class Program
{
    private static string ApplicationTitle = "playpodcast v0.01 | Copyright 2025, Matthew Hickson | https://github.com/mwhickson/playpodcast.git";

    private static Utility utility = new();

    private static Cli _cli = new(new CliTheme(
        new CliTheme.ColorPair(ConsoleColor.DarkBlue, ConsoleColor.White),
        new CliTheme.ColorPair(ConsoleColor.Red, ConsoleColor.White),
        new CliTheme.ColorPair(ConsoleColor.DarkBlue, ConsoleColor.Yellow),
        new CliTheme.ColorPair(ConsoleColor.Yellow, ConsoleColor.Black)
    ));

    private static DataStore db = new(utility.DefaultStoreFile);

    private static string SubscriptionFile = "";

    private static List<Podcast> podcasts = [];
    private static Podcast? selectedPodcast = null;

    private static List<Episode> episodes = [];
    private static Episode? selectedEpisode = null;

    private static Player? ThePlayer;

    private static void Main()
    {
        Setup();

        DisplayProgramTitle();

        _cli.Run();
        Cli.RestoreState();
    }

    private static Task<CliActionResult> QuitApplication(List<string> Options) {
        Environment.Exit(0);

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> DisplayHelp(List<string> Options) {
        if (CliActions != null) {
            CliActions.ForEach((a) => Console.WriteLine("{0} | {1} | {2}", a.Name, a.Description, string.Join(", ", a.Commands)));
        }

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> ChoosePodcast(List<string> Options) {
        if (Options.Count > 0)
        {
            int requestedPodcastIndex = Convert.ToInt32(Options.First());
            if (requestedPodcastIndex > 0 && requestedPodcastIndex <= podcasts.Count)
            {
                selectedPodcast = podcasts[requestedPodcastIndex - 1];

                Console.WriteLine("retrieving episodes for [{0}]...", selectedPodcast.Title);

                List<Episode> podcastEpisodes = Utility.GetEpisodesFromFeed(selectedPodcast);
                podcastEpisodes.ForEach((e) => db.Episodes.InsertOrUpdate(selectedPodcast, e));
                episodes = db.Episodes.GetListByPodcastId(selectedPodcast.Id);

                selectedPodcast.UpdatedOn = DateTime.Now;
                db.Podcasts.InsertOrUpdate(selectedPodcast);

                _cli.ThePrompt = selectedPodcast.Title;
            }
        }

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> ClearScreen(List<string> Options) {
        Console.Clear();
        DisplayProgramTitle();

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> ListEpisodes(List<string> Options) {
        if (selectedPodcast != null)
        {
            // episodes = Utility.GetEpisodesFromFeed(selectedPodcast);

            if (episodes.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Episode List [{0}]:", selectedPodcast.Title);
                Console.WriteLine();

                episodes.ForEach((e) => Console.WriteLine("{0} | {1} | {2}", e.Id, e.PublishedOn.ToShortDateString(), e.Title));
            }
            else
            {
                Console.WriteLine("No episodes found!");
            }
        }

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> ListPodcasts(List<string> Options) {
        Console.WriteLine();
        Console.WriteLine("Subscription List:");
        Console.WriteLine();

        podcasts.ForEach((p) => Console.WriteLine("{0} >> {1}", p.Id, p.Title));

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> PlayEpisode(List<string> Options) {
        if (Options.Count > 0)
        {
            int requestedEpisodeIndex = Convert.ToInt32(Options.First());
            if (requestedEpisodeIndex > 0 && requestedEpisodeIndex <= episodes.Count)
            {
                selectedEpisode = episodes[requestedEpisodeIndex - 1];
                _cli.ThePrompt = string.Format("{0} :: {1}", selectedPodcast?.Title, selectedEpisode.Title);

                ThePlayer = new(selectedEpisode.Url);
                ThePlayer.Play();
            }
        }

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static Task<CliActionResult> StopPlayback(List<string> Options) {
        if (ThePlayer != null)
        {
            ThePlayer.Stop();
        }

        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    private static List<CliAction> CliActions = [
        new CliAction("Quit", "Quit playpodcast", ["quit", "q"], QuitApplication),
        new CliAction("Help", "Display help", [ "help", "h", "?" ], DisplayHelp),
        new CliAction("Choose", "Choose a podcast", [ "choose", "c", "*" ], ChoosePodcast),
        new CliAction("Clear", "Clear the screen", [ "clear", "z" ], ClearScreen),
        new CliAction("Episodes", "List episodes", [ "episodes", "e" ], ListEpisodes),
        new CliAction("History", "View listening history", [ "history", "v" ], CliAction.DefaultSuccessFunction),
        new CliAction("Information", "Show podcast/episode information", [ "info", "i" ], CliAction.DefaultSuccessFunction),
        new CliAction("Podcasts", "List podcasts", [ "list", "l" ], ListPodcasts),
        new CliAction("Play", "Play an episode", [ "play", "p" ], PlayEpisode),
        new CliAction("Search", "Search for a podcast", [ "search", "s", "/" ], CliAction.DefaultSuccessFunction),
        new CliAction("Stop", "Stop playback", [ "stop", "x" ], StopPlayback),
        new CliAction("Subscribe", "Subscribe to podcast", [ "subscribe", "+" ], CliAction.DefaultSuccessFunction),
        new CliAction("Unsubscribe", "Unsubscribe from podcast", [ "unsubscribe", "-" ], CliAction.DefaultSuccessFunction),
    ];

    private static void GetPodcasts(string subscriptionFile)
    {
        List<Podcast> podcastsFromFile = Utility.LoadPodcastsFromFile(subscriptionFile);
        Utility.StorePodcasts(db, podcastsFromFile);
        podcasts = db.Podcasts.GetList();
    }

    private static void DisplayProgramTitle()
    {
        Console.WriteLine(ApplicationTitle);
    }

    private static void Setup()
    {
        SubscriptionFile = utility.DefaultSubscriptionFile;
        GetPodcasts(SubscriptionFile);

        bool storeIsValid = db.ValidateStore();
        
        CliActions.ForEach((a) => Cli.RegisterCommandHandler(a));

        Console.Clear(); // prime the pump to ensure our background covers the entire screen
    }
}
