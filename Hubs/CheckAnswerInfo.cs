namespace quiz_web_app.Hubs
{
    public class CheckAnswerInfo
    {
        public Guid CardId { get; set; }
        public List<Guid> Answers { get; set; } = null!;
    }
}
