using Internal;

namespace quiz_web_app.Hubs
{
    public interface IClient
    {
        Task ReceiveMessage(Message message);
        Task ReceiveQuestion(GetQuizCardDto answer);
        Task ReceiveAnswer(AnswerInfo info);
        Task GameStarts(MatchStartsInfo info);
        Task GameEnds(MatchEndsInfo info);
    }
}
