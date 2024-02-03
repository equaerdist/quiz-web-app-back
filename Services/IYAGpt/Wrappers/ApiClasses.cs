namespace quiz_web_app.Services.IYAGpt.Wrappers
{
    public class YAGptResponse
    {
        public YAGptResult Result { get; set; } = null!;
    }
    public class YAGptResult
    {
        public List<Alternative> Alternatives { get; set; } = null!;
        public UsageInfo Usage { get; set; } = null!;
        public string ModelVersion { get; set; } = null!;

    }
    public class UsageInfo
    {
        public int InputTextTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class Message
    {
        public string Role { get; set; } = null!;
        public string Text { get; set; } = null!;
    }

    public class Alternative
    {
        public Message Message { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
