using System.Xml;

namespace playpodcast;

public class Utility
{
    public static string DefaultStoreFilename = "playpodcast.db";
    public const string DefaultSubscriptionFilename = "subscriptions.opml";
    private const string ApplicationFolder = "playpodcast";
    private const string UserAgent = "Mozilla/9.9 (github.com/mwhickson/playpodcast) Chrome/999.9.9.9 Gecko/99990101 Firefox/999 Safari/999.9";

    private string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    
    public string DefaultSubscriptionFolder
    {
        get { return System.IO.Path.Join(UserFolder, ApplicationFolder); }
    }

    public string DefaultStoreFile
    {
        get { return System.IO.Path.Join(DefaultSubscriptionFolder, DefaultStoreFilename); }
    }

    public string DefaultSubscriptionFile
    {
        get { return System.IO.Path.Join(DefaultSubscriptionFolder, DefaultSubscriptionFilename); }
    }

    public static List<Podcast> LoadPodcastsFromFile(string filename)
    {
        List<Podcast> podcasts = new();

        if (File.Exists(filename))
        {
            XmlReaderSettings settings = GetFeedParserSettings();

            using (var reader = XmlReader.Create(filename, settings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "outline") continue;

                    string title = reader.GetAttribute("text") ?? "";
                    string url = reader.GetAttribute("xmlUrl") ?? "";

                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        podcasts.Add(new Podcast(title.Trim(), url.Trim()));
                    }
                }
            }
        }

        return podcasts;
    }

    public static bool StorePodcasts(DataStore store, List<Podcast> podcasts)
    {
        bool success = true;
        podcasts.ForEach((p) => success = success && store.Podcasts.InsertOrUpdate(p));
        return success;
    }

    public static List<Episode> GetEpisodesFromFeed(Podcast p)
    {
        List<Episode> episodes = new();

        if (!string.IsNullOrWhiteSpace(p.Url))
        {
            Uri url = new(p.Url);

            if (url.IsWellFormedOriginalString())
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

                HttpResponseMessage response = Task.Run(() => client.GetAsync(url)).Result;
                string content = Task.Run(() => response.Content.ReadAsStringAsync()).Result;

                if (content.Trim().Length > 0)
                {
                    XmlReaderSettings settings = GetFeedParserSettings();

                    StringReader contentReader = new(content);
                    using (var reader = XmlReader.Create(contentReader, settings))
                    {
                        int episodeOrder = 0;

                        while (reader.Read())
                        {
                            if (reader.NodeType != XmlNodeType.Element || reader.Name != "item") continue;

                            episodeOrder++;

                            string title = "";
                            string episodeUrl = "";
                            string sortKey = episodeOrder.ToString(); // NOTE: date would be better here, but the order in the feed is probably safe... (maybe even more reliable)

                            using (var itemReader = reader.ReadSubtree())
                            {
                                while (itemReader.Read())
                                {
                                    if (itemReader.NodeType != XmlNodeType.Element) continue;
                                    if (itemReader.Name == "title")
                                    {
                                        title = itemReader.ReadInnerXml();
                                        // FIX: stop the CDATA shenanigans...
                                        if (title.StartsWith("<![CDATA[", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            var cdataopen = "<![CDATA[".Length;
                                            var cdataclose = "]]>".Length;
                                            title = title.Substring(cdataopen, title.Length - cdataopen - cdataclose).Trim();
                                        }
                                    }

                                    if (itemReader.Name == "enclosure" && itemReader.GetAttribute("url") != null)
                                    {
                                        episodeUrl = itemReader.GetAttribute("url") ?? "";
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(episodeUrl)) continue;

                            Episode e = new Episode(title.Trim(), episodeUrl.Trim(), sortKey);
                            episodes.Add(e);
                        }
                    }
                }
            }
        }

        return episodes;
    }

    public static XmlReaderSettings GetFeedParserSettings()
    {
        XmlReaderSettings settings = new()
        {
            Async = false,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            ValidationType = ValidationType.None
        };

        return settings;
    }
}
