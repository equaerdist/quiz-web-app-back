namespace quiz_web_app.Infrastructure
{
    public class AppConfig
    {
        private string aws_access_key_id { get; set; } = null!;
        private string aws_secret_access_key { get; set; } = null!;
        private string yandex_iam_key { get; set; } = null!;

        public string YandexIAMKey => yandex_iam_key;
        public string AwsSecretKey => aws_secret_access_key;
        public string AwsAccessKey => aws_access_key_id;
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string ConnectionString { get; set; } = null!;
        public string SmtpServer { get; set; } = null!;
        public string SmtpPort { get; set;} = null!;
        public string EmailUsername { get; set; } = null!;
        public string EmailPassword { get; set; } = null!;
        public string FileStorage { get; set; } = null!;
        public string Gateway { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string catalogId { get; set; } = null!;
        public string RabbitHost { get; set; } = null!;
        public string RabbitPassword { get; set; } = null!;
        public string RabbitUser { get; set; } = null!; 
        public string RedisString { get; set; } = null!;
        public string QuizCardCachePrefix { get; set; } = "quizCard";
        public string QuizCachePrefix { get; set; } = "quiz";
        public string MatchEndsCachePrefix { get; set; } = "matchEnds";
        public string UserSessionPrefix { get; set; } = "userSession";
        public string GroupSessionPrefix { get; set; } = "groupSession";
    }
}
