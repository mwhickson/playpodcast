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

    public string OpmlFile { get; set; } = @"\projects\playpodcast\playpodcast\subscriptions.opml";
    public List<Tuple<string, string>> Podcasts { get; set; } = [];
    public DataTable PodcastTable { get; set; }
    public DataTable EpisodesTable { get; set; }

    public MainWindow()
    {
        ColorScheme = Colors.Base;

        Add(CreateMenu());
        Add(CreatePodcastsPane());
        Add(CreateEpisodesPane());
        Add(CreateStatusBar());
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

        TableView podcastTableView = new()
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

        podcastTableView.KeyUp += OnPodcastSelect;
        podcastTableView.Table = OnPodcastsPopulate();

        podcastPane.Add(podcastTableView);

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

        TableView episodeTableView = new()
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

        EpisodesTable.Rows.Add(["A sample episode", "1 minute", DateTime.Now.ToShortTimeString()]);

        episodeTableView.Table = EpisodesTable;

        episodePane.Add(episodeTableView);

        return episodePane;
    }

    private StatusBar CreateStatusBar()
    {
        StatusBar MainStatus = new()
        {
            Visible = true,
        };

        MainStatus.Items = [
            new StatusItem(Key.Null, "CTRL+Q to Quit", () => {}),
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

        return PodcastTable;
    }

    private void OnPodcastSelect(View.KeyEventEventArgs e)
    {
        KeyEvent keyEvent = e.KeyEvent;
        if (keyEvent.Key == Key.Enter)
        {
            MessageBox.Query(
                "Selected podcast",
                "{ a podcast }",
                "_Ok"
            );

            e.Handled = true;
        }
    }
}
