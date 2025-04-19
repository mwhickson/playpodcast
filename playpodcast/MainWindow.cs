using System.Data;
using Terminal.Gui;

public class MainWindow : Toplevel
{
    public MainWindow()
    {
        ColorScheme = Colors.Base;

        MenuBar = new MenuBar(
            new MenuBarItem[] {
                new MenuBarItem(
                    "_File", new MenuItem[]
                    {
                        new MenuItem("_Quit", "", () => RequestStop(), null, null, Key.Q | Key.CtrlMask)
                    }),
                new MenuBarItem(
                    "_Help", new MenuItem[]
                    {
                        new MenuItem(
                            "_About",
                            "",
                            () => MessageBox.Query(
                                "About playpodcast",
                                "\nA podcast player."
                                    + "\n\ngithub.com/mwhickson/playpodcast"
                                    + "\n\n(c) 2025 Matthew Hickson"
                                    + "\n\n- using -"
                                    + "\nLibVLCSharp"
                                    + "\nTerminal.Gui",
                                "_Ok"
                            ),
                            null,
                            null,
                            Key.F1
                        )
                    }),
            }
        );

        Add(MenuBar);

        int menuHeight = 1;
        int statusHeight = 1;

        FrameView podcastPane = new("Podcasts")
        {
            X = 0,
            Y = menuHeight,
            Width = Dim.Fill(0),
            Height = Dim.Percent(40) - menuHeight,
            CanFocus = true,
        };

        TableView podcastTableView = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(0),
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

        DataTable podcastTable = new();
        podcastTable.Columns.AddRange(new DataColumn[] {
            new DataColumn("Name"),
            new DataColumn("Last Updated"),
        });

        podcastTable.Rows.Add(["A sample podcast 1", DateTime.Now.ToShortTimeString()]);

        podcastTableView.Table = podcastTable;

        podcastPane.Add(podcastTableView);

        Add(podcastPane);

        FrameView episodePane = new("Episodes")
        {
            X = 0,
            Y = Pos.Percent(40),
            Width = Dim.Fill(0),
            Height = Dim.Fill(0) - statusHeight,
            CanFocus = true,
        };

        TableView episodeTableView = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(0),
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

        DataTable episodeTable = new();
        episodeTable.Columns.AddRange(new DataColumn[] {
            new DataColumn("Name"),
            new DataColumn("Duration"),
            new DataColumn("Published"),
        });

        episodeTable.Rows.Add(["A sample episode", "1 minute", DateTime.Now.ToShortTimeString()]);

        episodeTableView.Table = episodeTable;

        episodePane.Add(episodeTableView);

        Add(episodePane);

        StatusBar = new StatusBar()
        {
            Visible = true,
        };

        StatusBar.Items = new StatusItem[] {
            new StatusItem(Key.Null, "CTRL+Q to Quit", () => {}),
        };

        Add(StatusBar);
    }
}
