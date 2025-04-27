namespace playpodcast;

public class Episode
{
    public int Id { get; set; }
    public int PodcastId { get; }
    public string Title { get; set; }
    public string Url { get; }
    public DateTime PublishedOn { get; set; }
    public bool IsPlayed { get; set; }
    public int Position { get; set; }

    public Episode(int podcastId, string title, string url)
    {
        Id = 0;
        PodcastId = podcastId;
        Title = title;
        Url = url;
        PublishedOn = DateTime.MinValue;
        IsPlayed = false;
        Position = 0;
    }
}
