namespace playpodcast;

internal static class Program
{
    private static Utility utility = new();

    private static string SubscriptionFile = "";

    private static List<Podcast> podcasts = [];
    private static Podcast? selectedPodcast = null;

    private static List<Episode> episodes = [];
    private static Episode? selectedEpisode = null;

    private static string DefaultPrompt = "playpodcast> ";

    private static string ReadPrompt(string prompt)
    {
        Console.Write(prompt);
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
            case "clear":
            case "cl":
                Console.Clear();
                DisplayProgramTitle();
                break;

            case "episodes":
            case "ep":
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

            case "list":
            case "li":
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

            case "pick":
            case "pi":
                if (options.Count > 0)
                {
                    int requestedPodcastIndex = Convert.ToInt32(options.First());
                    if (requestedPodcastIndex > 0 && requestedPodcastIndex <= podcasts.Count)
                    {
                        selectedPodcast = podcasts[requestedPodcastIndex - 1];
                        Console.WriteLine("Podcast [{0}] selected.", selectedPodcast.Title);
                    }
                }
                break;

            case "play":
            case "pl":
                if (options.Count > 0)
                {
                    int requestedEpisodeIndex = Convert.ToInt32(options.First());
                    Console.WriteLine(requestedEpisodeIndex);
                    if (requestedEpisodeIndex > 0 && requestedEpisodeIndex <= episodes.Count)
                    {
                        selectedEpisode = episodes[requestedEpisodeIndex - 1];
                        Console.WriteLine("Playing [{0}]...", selectedEpisode.Title);

                        Player player = new(selectedEpisode.Url);
                        player.Play();
                    }
                }

                break;

            case "quit":
            case "q":
                Environment.Exit(0);
                break;

            case "help": // fall-through
            case "h":
            default:
                Console.WriteLine("Available Commands: quit, help, episodes, list, pick, play");
                break;
        }
    }

    private static void DisplayProgramTitle()
    {
        Console.WriteLine("playpodcast v0.01 | Copyright 2025, Matthew Hickson | https://github.com/mwhickson/playpodcast.git");
        Console.WriteLine();
    }

    private static void Setup()
    {
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
            string input = ReadPrompt(DefaultPrompt);

            List<string> parts = new(input.Split(" "));
            string command = parts.Count > 0 ? parts[0] : "";
            List<string> options = parts.Count > 1 ? new(parts.Skip(1)) : [];

            ProcessCommand(command, options);
        }
    }
}
