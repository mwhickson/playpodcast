namespace playpodcast;

internal static class Program
{
    private static void Main()
    {
        Utility utility = new();

        string subscriptionFile = utility.DefaultSubscriptionFile;

        Console.WriteLine("playpodcast v0.01 | Copyright 2025, Matthew Hickson | https://github.com/mwhickson/playpodcast.git");
        Console.WriteLine("Subscription File: {0}", subscriptionFile);

        List<Podcast> podcasts = Utility.LoadPodcastsFromFile(subscriptionFile);

        int podcastIndex = 0;
        podcasts.ForEach((p) =>
        {
            podcastIndex++;
            Console.WriteLine("{0}. [{1}]({2})", podcastIndex, p.Title, p.Url);
        });

        // Player player = new();
    }
}
