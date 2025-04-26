namespace playpodcast;

public class CliState
{
    public ConsoleColor Background { get; }
    public ConsoleColor Foreground { get; }

    public CliState(ConsoleColor background, ConsoleColor foreground)
    {
        Background = background;
        Foreground = foreground;
    }
}