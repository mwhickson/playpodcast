namespace playpodcast;

public class CliActionResult 
{ 
    public enum Result {
        Error,
        Success,
    }

    public enum MessageType {
        Data,
        Error,
        Information,
    }

    public class Message 
    { 
        public MessageType Type { get; }
        public string Text { get; }
        public Message (MessageType type, string text)
        {
            Type = type;
            Text = text;
        }
    }

    public Result FinalResult { get; }
    public List<Message> Messages { get; }

    public CliActionResult(Result result, List<Message> messages)
    {
        FinalResult = result;
        Messages = messages;
    }
}
