namespace quiz_web_app.Hubs
{
    public class MatchEndsInfo
    {
        public int Score { get; set; }
        public int AmountOfRightAnswers { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}