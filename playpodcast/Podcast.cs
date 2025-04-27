namespace playpodcast;

public class Podcast
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Url { get; }
    public string Description { get; set; }
    public List<Episode> Episodes { get; }
    public DateTime SubscribedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public int EpisodeCount
    {
        get { return Episodes.Count; }
    }

    public Podcast(string title, string url)
    {
        Id = 0;
        Title = title;
        Url = url;
        Description = "";
        Episodes = new();
        SubscribedOn = DateTime.MinValue;
        UpdatedOn = DateTime.MinValue;
    }

    public Podcast(string url) : this("", url) { }

    public void AddEpisode(Episode e)
    {
        Episodes.Add(e);
    }

    public void RemoveEpisode(Episode e)
    {
        Episodes.Remove(e);
    }
}
