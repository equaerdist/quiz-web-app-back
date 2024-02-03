namespace quiz_web_app.Services.BackgroundServices
{
    public class ConfigureEnvironment : BackgroundService
    {
        private readonly IWebHostEnvironment _env;

        public ConfigureEnvironment(IWebHostEnvironment env) { _env = env; }
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(() =>
        {
            var cardsPath = Path.Combine(_env.ContentRootPath, "Cards");
            var quizPath = Path.Combine(_env.ContentRootPath, "Quizes");
            var questionsPath = Path.Combine(_env.ContentRootPath, "Questions");
            if(!Directory.Exists(cardsPath))
                Directory.CreateDirectory(cardsPath);
            if(!Directory.Exists(quizPath))
                Directory.CreateDirectory(questionsPath);
            if (Directory.Exists(questionsPath))
                Directory.CreateDirectory(questionsPath);
        }, stoppingToken);
    }
}
