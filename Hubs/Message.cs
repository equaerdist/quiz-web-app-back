namespace quiz_web_app.Hubs
{
    public enum MessageType 
    { 
        Succesfull,
        Error
    }
    public enum NotifyType
    {
        Main,
        Submain
    }
    public enum AdditionActions
    { 
        None,
        ResetGroupInfo
    }

    public class Message
    {
        public string Content { get; set; } = null!;
        public MessageType Type { get; set; }
        public NotifyType NotifyType { get; set; }
        public AdditionActions AdditionActions { get; set; }
    }
}
