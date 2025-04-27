namespace playpodcast;

public class Episode
{
    public int Id { get; set; } = 0;
    public string Title { get; set; } = "";
    public string Url { get; }

    public Episode(string title, string url)
    {
        Title = title;
        Url = url;
    }
}
