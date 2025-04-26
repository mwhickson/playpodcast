namespace playpodcast;

public delegate Task<CliActionResult> ActionHandler(List<string> Options);


public class CliAction
{ 
    public static Task<CliActionResult> DefaultSuccessFunction(List<string> Options) {
        CliActionResult result = new(CliActionResult.Result.Success, []);
        return Task.Run(() => result);
    }

    public static Task<CliActionResult> DefaultErrorFunction(List<string> Options) {
        CliActionResult result = new(CliActionResult.Result.Success, [new CliActionResult.Message(CliActionResult.MessageType.Error, "an error has occurred")]);
        return Task.Run(() => result);
    }

    public string Name { get; }
    public string Description { get; }
    public List<string> Commands { get; }
    public ActionHandler Handler { get; }
    
    public CliAction(string name, string description, List<string> commands, ActionHandler handler)
    {
        Name = name;
        Description = description;
        Commands = commands;
        Handler = handler;
    }
    
    public CliAction(List<string> commands, ActionHandler handler): this("", "", commands, handler) { }

    public Task<CliActionResult> Execute(List<string> options)
    {
        return Handler(options);
    }
}
