namespace playpodcast;

internal static class Program
{
    private static Utility utility = new();

    private static string SubscriptionFile = "";

    private static List<Podcast> podcasts = [];
    private static Podcast? selectedPodcast = null;

    private static List<Episode> episodes = [];
    private static Episode? selectedEpisode = null;

    private static Player? ThePlayer;

    private static string DefaultPrompt = "enter command";
    private static string ThePrompt = DefaultPrompt;

    private static string ReadPrompt()
    {
        Console.WriteLine();
        Console.WriteLine("[{0}]", ThePrompt);
        Console.Write("> ");

        string? input = Console.ReadLine();

        return input ?? "";
    }

    private static void GetPodcasts(string subscriptionFile)
    {
        podcasts = Utility.LoadPodcastsFromFile(subscriptionFile);
    }

    private static void ProcessCommand(string command, List<string> options)
    {
        switch (command.ToLower())
        {
            case "choose":
            case "c":
            case "*":
                if (options.Count > 0)
                {
                    int requestedPodcastIndex = Convert.ToInt32(options.First());
                    if (requestedPodcastIndex > 0 && requestedPodcastIndex <= podcasts.Count)
                    {
                        selectedPodcast = podcasts[requestedPodcastIndex - 1];
                        ThePrompt = selectedPodcast.Title;
                    }
                }
                break;

            case "clear":
            case "z":
                Console.Clear();
                DisplayProgramTitle();
                break;

            case "episodes":
            case "e":
                if (selectedPodcast != null)
                {
                    episodes = Utility.GetEpisodesFromFeed(selectedPodcast);

                    if (episodes.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Episode List [{0}]:", selectedPodcast.Title);
                        Console.WriteLine();

                        episodes.ForEach((e) => Console.WriteLine("{0}. {1}", e.SortKey, e.Title));
                    }
                    else
                    {
                        Console.WriteLine("No episodes found!");
                    }
                }
                break;

            case "history":
            case "v":
                // TODO:
                break;

            case "info":
            case "i":
                // TODO:
                break;

            case "list":
            case "l":
                if (options.Count > 0)
                {
                    string listOptions = options.First().ToString();

                    switch (listOptions.ToLower())
                    {
                        case "refresh":
                        case "r":
                            GetPodcasts(SubscriptionFile);
                            Console.WriteLine("Subscriptions reloaded.");
                            break;
                        default:
                            // PASS:
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Subscription List [{0}]:", SubscriptionFile);
                Console.WriteLine();

                int podcastIndex = 0;
                podcasts.ForEach((p) =>
                {
                    podcastIndex++;
                    Console.WriteLine("{0}. {1}", podcastIndex, p.Title);
                });

                break;

            case "play":
            case "p":
                if (options.Count > 0)
                {
                    int requestedEpisodeIndex = Convert.ToInt32(options.First());
                    if (requestedEpisodeIndex > 0 && requestedEpisodeIndex <= episodes.Count)
                    {
                        selectedEpisode = episodes[requestedEpisodeIndex - 1];
                        ThePrompt = string.Format("{0} :: {1}", selectedPodcast?.Title, selectedEpisode.Title);

                        ThePlayer = new(selectedEpisode.Url);
                        ThePlayer.Play();
                    }
                }

                break;

            case "quit":
            case "q":
                Environment.Exit(0);
                break;

            case "search":
            case "s":
            case "/":
                // TODO:
                break;

            case "stop":
            case "x":
                if (ThePlayer != null)
                {
                    ThePlayer.Stop();
                }
                break;

            case "subscribe":
            case "+":
                // TODO:
                break;

            case "unsubscribe":
            case "u":
            case "-":
                // TODO:
                break;

            case "help": // fall-through
            case "h":
            case "?":
            default:
                // TODO: use a better table formatting solution...
                List<string> commandList = new()
                {
                    "Quit:                      'quit'         : 'q'",
                    "Help:                      'help'         : '?' or 'h'",
                    "Choose a podcast:          'choose'       : '*' or 'c'  | param: {int} podcast index",
                    "Clear screen:              'clear'        : 'z'",
                    "List episodes:             'episodes'     : 'e'",
                    "Show listening history:    'history'      : 'v'",
                    "Show podcast/episode info: 'info'         : 'i'",
                    "List podcasts:             'list'         : 'l'         | param: {string} 'refresh' : 'r'",
                    "Play an episode:           'play'         : 'p'         | param: {int} episode index",
                    "Search for a podcast:      'search'       : '/' or 's'  | param: {string} text to search for",
                    "Stop playback:             'stop'         : 'x'",
                    "Subscribe to podcast:      'subscribe'    : '+'         | param: {int} podcast index from search results",
                    "Unsubscribe from podcast:  'unsubscribe'  : '-' or 'u'  | param: {int} podcast index",
                };

                Console.WriteLine("Available Commands:");
                commandList.ForEach((string helpItem) => Console.WriteLine(helpItem));
                Console.WriteLine();

                break;
        }
    }

    private static void DisplayProgramTitle()
    {
        Console.WriteLine("playpodcast v0.01 | Copyright 2025, Matthew Hickson | https://github.com/mwhickson/playpodcast.git");
    }

    private static void Setup()
    {
        ThePrompt = DefaultPrompt;
        GetPodcasts(SubscriptionFile);
    }

    private static void Main()
    {
        Console.Clear();

        SubscriptionFile = utility.DefaultSubscriptionFile;
        Setup();

        DisplayProgramTitle();

        while (true)
        {
            string input = ReadPrompt();

            List<string> parts = new(input.Split(" "));
            string command = parts.Count > 0 ? parts[0] : "";
            List<string> options = parts.Count > 1 ? new(parts.Skip(1)) : [];

            ProcessCommand(command, options);
        }
    }
}
