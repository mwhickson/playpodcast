namespace playpodcast;

public class Podcast
{
    public int Id { get; set; } = 0;
    public string Title { get; set; }
    public string Url { get; }
    public List<Episode> Episodes { get; }
    public DateTime UpdatedOn { get; set; }
    public int EpisodeCount
    {
        get { return Episodes.Count; }
    }

    public Podcast(string title, string url)
    {
        Title = title;
        Url = url;
        Episodes = new();
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
