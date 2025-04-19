using Terminal.Gui;

public class MainWindow: Toplevel
{
    public MainWindow() {
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
                        new MenuItem("_About", "", () => MessageBox.Query("About playpodcast", "TODO...", "_Ok"), null, null, Key.F1)
                    }),
            }
        );

        Add(MenuBar);

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
