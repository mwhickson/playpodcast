using System.Data;
using System.Xml;
using Terminal.Gui;

public class MainWindow : Toplevel
{
    private static readonly int menuHeight = 1;
    private static readonly int statusHeight = 1;

    private static readonly string fileMenuText = "_File";
    private static readonly string fileNewMenuText = "_New from URL...";
    private static readonly string fileOpenMenuText = "_Open OPML...";
    private static readonly string fileQuitMenuText = "_Quit";
    private static readonly string helpMenuText = "_Help";
    private static readonly string helpAboutMenuText = "_About";
    private static readonly string podcastsTitle = "Podcasts";
    private static readonly string episodesTitle = "Episodes";

    private string OpmlFile { get; set; } = @"\projects\playpodcast\playpodcast\subscriptions.opml";
    private List<Tuple<string, string>> Podcasts { get; set; } = [];
    private List<Tuple<string, string>> Episodes { get; set; } = [];
    private Tuple<string, string> SelectedPodcast { get; set; }
    private Tuple<string, string> SelectedEpisode { get; set; }
    private TableView PodcastView { get; set; }
    private TableView EpisodeView { get; set; }
    public DataTable PodcastTable { get; set; }
    public DataTable EpisodesTable { get; set; }
    public StatusBar MainStatus {get; set; }

    public MainWindow()
    {
        ColorScheme = Colors.Base;

        Add(CreateMenu());
        Add(CreateStatusBar());
        Add(CreatePodcastsPane());
        Add(CreateEpisodesPane());
    }

    private MenuBar CreateMenu()
    {
        MenuBar MainMenu = new(
            [
                new MenuBarItem(
                    fileMenuText,
                    [
                        new MenuItem(fileNewMenuText, "", OnNewFromURL, null, null, Key.N | Key.CtrlMask),
                        new MenuItem(fileOpenMenuText, "", OnOpenFromOPML, null, null, Key.O | Key.CtrlMask),
                        new MenuItem(fileQuitMenuText, "", OnQuit, null, null, Key.Q | Key.CtrlMask),
                    ]),
                new MenuBarItem(
                    helpMenuText,
                    [
                        new MenuItem(helpAboutMenuText, "", OnShowAbout, null, null, Key.F1),
                    ]),
            ]
        );

        return MainMenu;
    }

    private FrameView CreatePodcastsPane()
    {
        FrameView podcastPane = new(podcastsTitle)
        {
            X = 0,
            Y = menuHeight,
            Width = Dim.Fill(0),
            Height = Dim.Percent(40) - menuHeight,
            CanFocus = true,
        };

        PodcastView = new()
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Percent(100),
            FullRowSelect = true,
            Style = new TableView.TableStyle()
            {
                AlwaysShowHeaders = true,
                ShowHorizontalHeaderOverline = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
                ShowVerticalHeaderLines = false,
            },
        };

        PodcastView.KeyUp += OnPodcastSelect;
        PodcastView.Table = OnPodcastsPopulate();

        podcastPane.Add(PodcastView);

        return podcastPane;
    }

    private FrameView CreateEpisodesPane()
    {
        FrameView episodePane = new(episodesTitle)
        {
            X = 0,
            Y = Pos.Percent(40),
            Width = Dim.Fill(0),
            Height = Dim.Fill(0) - statusHeight,
            CanFocus = true,
        };

        EpisodeView = new()
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Percent(100),
            FullRowSelect = true,
            Style = new TableView.TableStyle()
            {
                AlwaysShowHeaders = true,
                ShowHorizontalHeaderOverline = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
                ShowVerticalHeaderLines = false,
            },
        };

        EpisodesTable = new();
        EpisodesTable.Columns.AddRange([
            new DataColumn("Name"),
            new DataColumn("Duration"),
            new DataColumn("Published"),
        ]);

        EpisodeView.KeyUp += OnEpisodeSelect;
        EpisodeView.Table = EpisodesTable;

        episodePane.Add(EpisodeView);

        return episodePane;
    }

    private StatusBar CreateStatusBar()
    {
        MainStatus = new()
        {
            Visible = true,
        };

        MainStatus.Items = [
            new StatusItem(Key.Null, "ready...", () => {}),
        ];

        return MainStatus;
    }

    private void OnQuit()
    {
         RequestStop();
    }

    private void OnNewFromURL()
    {
        // TODO:
    }

    private void OnOpenFromOPML()
    {
        // TODO:
    }

    private void OnShowAbout()
    {
        MessageBox.Query(
            "About playpodcast",
            "\nA podcast player."
                + "\n\ngithub.com/mwhickson/playpodcast"
                + "\n\n(c) 2025 Matthew Hickson"
                + "\n\n- using -"
                + "\nLibVLCSharp"
                + "\nTerminal.Gui",
            "_Ok"
        );
    }

    private DataTable OnPodcastsPopulate()
    {
        UpdateStatus("Populating Podcast list from OPML...");

        PodcastTable = new();
        PodcastTable.Columns.AddRange([
            new DataColumn("Name"),
        ]);

        XmlReaderSettings xmlsettings = new()
        {
            Async = false,
            IgnoreComments = true,
            IgnoreWhitespace = true,
            ValidationType = ValidationType.None,
        };

        this.PodcastView.Clear();
        PodcastTable.Clear();
        Podcasts.Clear();
        using (XmlReader reader = XmlReader.Create(OpmlFile, xmlsettings))
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "outline")
                {
                    string PodcastTitle = reader.GetAttribute("text") ?? "";
                    string PodcastUrl = reader.GetAttribute("xmlUrl") ?? "";

                    if (!string.IsNullOrWhiteSpace(PodcastTitle) && !string.IsNullOrWhiteSpace(PodcastUrl))
                    {
                        Tuple<string, string> podcast = new(PodcastTitle, PodcastUrl);
                        Podcasts.Add(podcast);
                        PodcastTable.Rows.Add([podcast.Item1]);
                    }
                }
            }
        }

        this.PodcastView.SetNeedsDisplay();
        UpdateStatus(string.Format("{0} podcasts loaded...", Podcasts.Count));

        return PodcastTable;
    }

    private Task GetEpisodes(string feedUrl)
    {
        if (!string.IsNullOrWhiteSpace(feedUrl))
        {
            UpdateStatus("Loading episodes...");

            Uri url = new(feedUrl);

            this.EpisodeView.Clear();
            EpisodesTable.Clear();
            Episodes.Clear();

            int episodeCount = 0;

            string content = "";
            using (HttpClient client = new()) {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/9.9 (github.com/mwhickson/playpodcast) Chrome/999.9.9.9 Gecko/99990101 Firefox/999 Safari/999.9");

                HttpResponseMessage response = Task.Run(() => client.GetAsync(url)).Result;
                response.EnsureSuccessStatusCode();

                content = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
            }

            UpdateStatus("Processing feed...");

            XmlReaderSettings xmlsettings = new()
            {
                Async = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationType = ValidationType.None,
            };

            StringReader contentReader = new(content);
            string EpisodeTitle = "";
            string EpisodeUrl = "";
            using (XmlReader reader = XmlReader.Create(contentReader, xmlsettings))
            {

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "item")
                    {
                        episodeCount++;
                        EpisodeTitle = "";
                        EpisodeUrl = "";

                        using (XmlReader itemReader = reader.ReadSubtree())
                        {
                            while (itemReader.Read())
                            {
                                if (itemReader.NodeType == XmlNodeType.Element)
                                {
                                    if (itemReader.Name == "title") {
                                        EpisodeTitle = itemReader.ReadInnerXml() ?? "";
                                        if (EpisodeTitle.ToUpper().StartsWith("<![CDATA["))
                                        {
                                            int cdataopen = "<![CDATA[".Length;
                                            int cdataclose = "]]>".Length;
                                            EpisodeTitle = EpisodeTitle.Substring(cdataopen, EpisodeTitle.Length - cdataopen - cdataclose).Trim();
                                        }
                                    }

                                    if (itemReader.Name == "enclosure" && itemReader.GetAttribute("url") != null) {
                                        EpisodeUrl = itemReader.GetAttribute("url") ?? "";
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(EpisodeTitle) && !string.IsNullOrWhiteSpace(EpisodeUrl))
                        {
                            Tuple<string, string> episode = new(EpisodeTitle, EpisodeUrl);
                            Episodes.Add(episode);
                            EpisodesTable.Rows.Add([episode.Item1]);
                        }
                    }
                }
            }

            UpdateStatus(string.Format("Viewing {0} [{1} episodes]", SelectedPodcast.Item1, episodeCount));
        }

        this.EpisodeView.SetNeedsDisplay();

        return Task.CompletedTask;
    }

    private void UpdateStatus(string message)
    {
        MainStatus.Items[0].Title = message;
        MainStatus.SetNeedsDisplay();
    }

    private void OnPodcastSelect(View.KeyEventEventArgs e)
    {
        KeyEvent keyEvent = e.KeyEvent;
        if (keyEvent.Key == Key.Enter)
        {
            int SelectedRowIndex = this.PodcastView.SelectedRow;

            if (SelectedRowIndex < Podcasts.Count)
            {
                SelectedPodcast = Podcasts[SelectedRowIndex];
                GetEpisodes(SelectedPodcast.Item2);
            }

            e.Handled = true;
        }
    }

    private void OnEpisodeSelect(View.KeyEventEventArgs e)
    {
        KeyEvent keyEvent = e.KeyEvent;
        if (keyEvent.Key == Key.Enter)
        {
            int SelectedRowIndex = this.EpisodeView.SelectedRow;

            if (SelectedRowIndex < Episodes.Count)
            {
                SelectedEpisode = Episodes[SelectedRowIndex];

                UpdateStatus(string.Format("Playing {0}...", SelectedEpisode.Item1));

                PodcastEpisodePlayer player = new(SelectedEpisode.Item2);
                player.Play();
            }

            e.Handled = true;
        }
    }
}
