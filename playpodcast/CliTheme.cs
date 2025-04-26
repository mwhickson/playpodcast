namespace playpodcast;

public class CliTheme 
{ 
    public class ColorPair     
    { 
        public ConsoleColor Background { get; }
        public ConsoleColor Foreground { get; }
        public ColorPair (ConsoleColor background, ConsoleColor foreground)
        {
            Background = background;
            Foreground = foreground;
        }
    }    

    public static readonly ColorPair DefaultColorPair = new(ConsoleColor.Black, ConsoleColor.White);
    public static readonly CliTheme DefaultCliTheme = new(DefaultColorPair, DefaultColorPair, DefaultColorPair, DefaultColorPair);

    public ColorPair DefaultColors { get; }
    public ColorPair ErrorColors { get; }
    public ColorPair PrimaryHighlightColors { get; }
    public ColorPair SecondaryHighlightColors { get; }

    public CliTheme(ColorPair defaultColors, ColorPair errorColors, ColorPair primaryHighlightColors, ColorPair secondaryHighlightColors)
    {
        DefaultColors = defaultColors;
        ErrorColors = errorColors;
        PrimaryHighlightColors = primaryHighlightColors;
        SecondaryHighlightColors = secondaryHighlightColors;
    }
}
