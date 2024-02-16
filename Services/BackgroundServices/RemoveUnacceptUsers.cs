using MassTransit;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure.Consumers.QuizCreatedEventConsumer;

namespace quiz_web_app.Services.BackgroundServices
{
    public class RemoveUnacceptUsers : BackgroundService
    {
        private readonly ILogger<RemoveUnacceptUsers> _logger;
        private readonly TimeSpan _waitTime;
        private readonly IServiceProvider _serviceProvider;

        public RemoveUnacceptUsers(
            ILogger<RemoveUnacceptUsers> logger,
            IServiceProvider serviceProvider
            )
        {
            _logger = logger;
            _waitTime = TimeSpan.FromHours(2);
            _serviceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var _ctx = scope.ServiceProvider.GetRequiredService<QuizAppContext>();
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"Start unaccept users delete task");
                    var usersForDelete = _ctx.Users
                                            .Where(u => u.CreatedAt < DateTime.UtcNow - _waitTime 
                                                    && u.Accepted == false);
                    if (usersForDelete.Any())
                    {
                        _logger.LogInformation($"Total users to delete {usersForDelete.Count()}");
                        _ctx.Users.RemoveRange(usersForDelete);
                        await _ctx.SaveChangesAsync();
                    }
                    _logger.LogInformation($"Task is completed.");
                    await Task.Delay(_waitTime);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Background service for removing not accepted users is crashed!\n{ex.Message}");
            }
        });
        
    }
}
