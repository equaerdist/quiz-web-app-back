using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace quiz_web_app.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public virtual string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
