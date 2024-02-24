using Internal;
using static quiz_web_app.Hubs.Button;

namespace quiz_web_app.Hubs
{
    public interface IClient
    {
        Task ReceiveMessage(Message message);
        Task GameStarts(MatchStartsInfo info);
    }
}
