namespace playpodcast;

public class Episode
{
    public string Title { get; set; }
    public string Url { get; }
    public string SortKey { get; }

    public Episode(string title, string url, string sortKey)
    {
        Title = title;
        Url = url;
        SortKey = sortKey;
    }
}
