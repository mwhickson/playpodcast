using System.Data;
using System.Xml;
using Terminal.Gui;

namespace playpodcast;

public sealed class MainWindow : Toplevel
{
    private const int MenuHeight = 1;
    private const int StatusHeight = 1;

    private const string FileMenuText = "_File";
    private const string FileNewMenuText = "_New from URL...";
    private const string FileOpenMenuText = "_Open OPML...";
    private const string FileQuitMenuText = "_Quit";
    private const string ToolsMenuText = "_Tools";
    private const string ToolsFindMenuText = "_Find a podcast...";
    private const string HelpMenuText = "_Help";
    private const string HelpAboutMenuText = "_About";
    private const string PodcastsTitle = "Podcasts";
    private const string EpisodesTitle = "Episodes";

    private string OpmlFile { get; set; } = "";
    private List<Tuple<string, string?>?> Podcasts { get; } = [];
    private List<Tuple<string, string?>?> Episodes { get; } = [];
    private Tuple<string, string?>? SelectedPodcast { get; set; }
    private Tuple<string, string?>? SelectedEpisode { get; set; }
    private TableView? PodcastView { get; set; }
    private TableView? EpisodeView { get; set; }
    private DataTable PodcastTable { get; set; }
    private DataTable EpisodesTable { get; set; }
    private StatusBar MainStatus { get; set; }
    private PodcastEpisodePlayer? Player { get; set; }

    public MainWindow()
    {
        PodcastTable = new DataTable();
        EpisodesTable = new DataTable();
        MainStatus = new StatusBar();
            
        ColorScheme = Colors.Base;

        Add(CreateMenu());
        Add(CreateStatusBar());
        Add(CreatePodcastsPane());
        Add(CreateEpisodesPane());
    }

    private MenuBar CreateMenu()
    {
        MenuBar mainMenu = new(
            [
                new MenuBarItem(
                    FileMenuText,
                    [
                        new MenuItem(FileNewMenuText, "", OnNewFromURL, null, null, Key.N | Key.CtrlMask),
                        new MenuItem(FileOpenMenuText, "", OnOpenFromOPML, null, null, Key.O | Key.CtrlMask),
                        new MenuItem(FileQuitMenuText, "", OnQuit, null, null, Key.Q | Key.CtrlMask)
                    ]),
                new MenuBarItem(
                    ToolsMenuText,
                    [
                        new MenuItem(ToolsFindMenuText, "", OnFindPodcast, null, null, Key.F3)
                    ]),
                new MenuBarItem(
                    HelpMenuText,
                    [
                        new MenuItem(HelpAboutMenuText, "", OnShowAbout, null, null, Key.F1)
                    ])
            ]
        );

        return mainMenu;
    }

    private StatusBar CreateStatusBar()
    {
        MainStatus = new StatusBar
        {
            Visible = true
        };

        MainStatus.Items = [
            new StatusItem(Key.Null, "ready...", () => {})
        ];

        return MainStatus;
    }

    private FrameView CreatePodcastsPane()
    {
        FrameView podcastPane = new(PodcastsTitle)
        {
            X = 0,
            Y = MenuHeight,
            Width = Dim.Fill(),
            Height = Dim.Percent(40) - MenuHeight,
            CanFocus = true
        };

        PodcastView = new TableView
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Percent(100),
            FullRowSelect = true,
            Style = new TableView.TableStyle
            {
                AlwaysShowHeaders = true,
                ShowHorizontalHeaderOverline = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
                ShowVerticalHeaderLines = false
            }
        };

        PodcastView.KeyUp += OnPodcastSelect;
        
        PodcastTable = new DataTable();
        PodcastTable.Columns.AddRange([
            new DataColumn("Name")
        ]);
        PodcastView.Table = OnPodcastsPopulate();

        podcastPane.Add(PodcastView);

        return podcastPane;
    }

    private FrameView CreateEpisodesPane()
    {
        FrameView episodePane = new(EpisodesTitle)
        {
            X = 0,
            Y = Pos.Percent(40),
            Width = Dim.Fill(),
            Height = Dim.Fill() - StatusHeight,
            CanFocus = true
        };

        EpisodeView = new TableView
        {
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            Height = Dim.Percent(100),
            FullRowSelect = true,
            Style = new TableView.TableStyle
            {
                AlwaysShowHeaders = true,
                ShowHorizontalHeaderOverline = false,
                ShowHorizontalHeaderUnderline = false,
                ShowVerticalCellLines = false,
                ShowVerticalHeaderLines = false
            }
        };

        EpisodesTable = new DataTable();
        EpisodesTable.Columns.AddRange([
            new DataColumn("Name")
            // new DataColumn("Duration"),
            // new DataColumn("Published")
        ]);

        EpisodeView.KeyUp += OnEpisodeSelect;
        EpisodeView.Table = EpisodesTable;

        episodePane.Add(EpisodeView);

        return episodePane;
    }
    
    private void OnQuit()
    {
        RequestStop();
    }

    private static void ShowNotImplemented()
    {
        MessageBox.Query("Not implemented", "Not just yet...", "_Ok");
    }

    private static void OnNewFromURL()
    {
        ShowNotImplemented();
        // TODO:
    }

    private void OnOpenFromOPML()
    {
        OpenDialog dialog = new()
        {
            AllowedFileTypes = ["opml", "xml"],
            AllowsMultipleSelection = false,
            AutoSize = true,
            Message = "Select an OPML file...",
            Title = "Open OPML file",
            Visible = true
        };

        Application.Run(dialog);

        if (dialog.Canceled || string.IsNullOrWhiteSpace(dialog.FilePath?.ToString())) return;
        
        var fileName = dialog.FilePath.ToString();
        
        if (!File.Exists(fileName)) return;
        OpmlFile = fileName;
        OnPodcastsPopulate();
        PodcastView?.SetNeedsDisplay();
    }

    private static void OnFindPodcast()
    {
        ShowNotImplemented();
        // TODO: iTunes search
    }

    private static void OnShowAbout()
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
        PodcastView?.Clear();
        PodcastTable.Clear();
        Podcasts.Clear();

        if (File.Exists(OpmlFile))
        {
            UpdateStatus("Populating Podcast list from OPML...");

            XmlReaderSettings xmlsettings = new()
            {
                Async = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationType = ValidationType.None
            };

            using (var reader = XmlReader.Create(OpmlFile, xmlsettings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "outline") continue;
                    var podcastTitle = reader.GetAttribute("text") ?? "";
                    var podcastUrl = reader.GetAttribute("xmlUrl") ?? "";

                    if (string.IsNullOrWhiteSpace(podcastTitle) || string.IsNullOrWhiteSpace(podcastUrl)) continue;
                    UpdateStatus($"Adding podcast {podcastTitle}...");

                    Tuple<string, string?> podcast = new(podcastTitle, podcastUrl);
                    Podcasts.Add(podcast);
                    PodcastTable.Rows.Add(podcast.Item1);
                }
            }

            UpdateStatus($"{Podcasts.Count} podcasts loaded...");
        }

        PodcastView?.SetNeedsDisplay();

        return PodcastTable;
    }

    private void GetEpisodes(string? feedUrl)
    {
        if (!string.IsNullOrWhiteSpace(feedUrl))
        {
            UpdateStatus("Loading episodes...");

            Uri url = new(feedUrl);

            EpisodeView?.Clear();
            EpisodesTable.Clear();
            Episodes.Clear();

            var episodeCount = 0;

            HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/9.9 (github.com/mwhickson/playpodcast) Chrome/999.9.9.9 Gecko/99990101 Firefox/999 Safari/999.9");

            var response = Task.Run(() => client.GetAsync(url)).Result;
            response.EnsureSuccessStatusCode();

            var content = Task.Run(() => response.Content.ReadAsStringAsync()).Result;

            UpdateStatus("Processing feed...");

            XmlReaderSettings xmlsettings = new()
            {
                Async = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationType = ValidationType.None
            };

            StringReader contentReader = new(content);
            using (var reader = XmlReader.Create(contentReader, xmlsettings))
            {
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element || reader.Name != "item") continue;
                    
                    episodeCount++;
                    var episodeTitle = "";
                    var episodeUrl = "";

                    using (var itemReader = reader.ReadSubtree())
                    {
                        while (itemReader.Read())
                        {
                            if (itemReader.NodeType != XmlNodeType.Element) continue;
                            if (itemReader.Name == "title")
                            {
                                episodeTitle = itemReader.ReadInnerXml();
                                if (episodeTitle.StartsWith("<![CDATA[", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    var cdataopen = "<![CDATA[".Length;
                                    var cdataclose = "]]>".Length;
                                    episodeTitle = episodeTitle.Substring(cdataopen, episodeTitle.Length - cdataopen - cdataclose).Trim();
                                }
                            }

                            if (itemReader.Name == "enclosure" && itemReader.GetAttribute("url") != null)
                            {
                                episodeUrl = itemReader.GetAttribute("url") ?? "";
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(episodeTitle) || string.IsNullOrWhiteSpace(episodeUrl)) continue;
                    
                    UpdateStatus($"Adding episode {episodeTitle}...");

                    Tuple<string, string?> episode = new(episodeTitle, episodeUrl);
                    Episodes.Add(episode);
                    EpisodesTable.Rows.Add(episode.Item1);
                }
            }

            UpdateStatus($"Viewing {SelectedPodcast?.Item1} [{episodeCount} episodes]");
        }

        EpisodeView?.SetNeedsDisplay();
    }

    private void UpdateStatus(string message)
    {
        MainStatus.Items = [
            new StatusItem(Key.Null, message, null)
        ];
        MainStatus.SetNeedsDisplay();

        // MainStatus.Items[0].Title = message;
        // MainStatus.SetChildNeedsDisplay();
        // MainStatus.SetNeedsDisplay();
        // MainStatus.Redraw(MainStatus.Bounds);
    }

    private void OnPodcastSelect(KeyEventEventArgs e)
    {
        var keyEvent = e.KeyEvent;
        if (keyEvent.Key != Key.Space) return;
        if (PodcastView != null)
        {
            var selectedRowIndex = PodcastView.SelectedRow;

            if (selectedRowIndex >= 0 && selectedRowIndex < Podcasts.Count)
            {
                SelectedPodcast = Podcasts[selectedRowIndex];
                GetEpisodes(SelectedPodcast?.Item2);
            }
        }

        e.Handled = true;
    }

    private void OnEpisodeSelect(KeyEventEventArgs e)
    {
        var keyEvent = e.KeyEvent;
        if (keyEvent.Key != Key.Space) return;
        if (EpisodeView != null)
        {
            var selectedRowIndex = EpisodeView.SelectedRow;

            if (selectedRowIndex < Episodes.Count)
            {
                if (SelectedEpisode != null && !string.IsNullOrWhiteSpace(SelectedEpisode.Item2) &&  Player != null)
                {
                    Player.Stop();
                }

                SelectedEpisode = Episodes[selectedRowIndex];

                UpdateStatus($"Starting playback of episode {SelectedEpisode?.Item1}...");

                Player = new PodcastEpisodePlayer(SelectedEpisode?.Item2);
                Player.Play();

                UpdateStatus($"Playing {SelectedEpisode?.Item1}...");
            }
        }

        e.Handled = true;
    }
}