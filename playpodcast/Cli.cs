namespace playpodcast;

public class Cli
{
    private static string DefaultPrompt = "enter command";
    public string ThePrompt { get; set; } = DefaultPrompt;

    private static CliState? _savedState;
    private static CliTheme _theme = CliTheme.DefaultCliTheme; 
    private static List<CliAction> _actions = [];

    public Cli(CliTheme? theme = null)
    {
        SaveState();

        _theme = theme ?? CliTheme.DefaultCliTheme;
        ApplyTheme();
    }

    private static void SaveState() 
    {
        _savedState = new(Console.BackgroundColor, Console.ForegroundColor);        
    }

    public static void RestoreState()
    {
        if (_savedState != null)
        {
            Console.BackgroundColor = _savedState.Background;
            Console.ForegroundColor = _savedState.Foreground;
        }
    }

    public static void ApplyTheme()
    {
        Console.BackgroundColor = _theme.DefaultColors.Background;
        Console.ForegroundColor = _theme.DefaultColors.Foreground;
    }

    public static void RegisterCommandHandler(CliAction action)
    {
        _actions.Add(action);
    }

    public void ReadAndProcessUserInput()
    {
        Console.WriteLine();
        Console.WriteLine("[{0}]", ThePrompt);
        Console.Write("> ");

        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            List<string> parts = new(input.Split(" "));
            string command = parts.Count > 0 ? parts[0].ToLower() : "";
            List<string> options = parts.Count > 1 ? new(parts.Skip(1)) : [];

            CliAction? action = _actions.Find((a) => a.Commands.Contains(command));

            if (action != null)
            {
                CliActionResult result = action.Handler(options).Result;

                switch(result.FinalResult)
                {
                    case CliActionResult.Result.Error:
                        if (result.Messages.Count > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("*** ERROR ***");
                            result.Messages.ForEach((m) => Console.WriteLine(m.ToString()));
                        }
                        break;
                    case CliActionResult.Result.Success:
                        // PASS:
                        break;
                }
            }
        }
    }

    public void Run()
    {
        while (true)
        {
            ReadAndProcessUserInput();
        }
    }
}
