using Amazon.Util.Internal;
using quiz_web_app.Infrastructure;
using quiz_web_app.Services.IYAGpt.Wrappers;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace quiz_web_app.Services.IYAGpt
{
    public partial class YAGptClient : IYAGpt
    {
        private readonly HttpClient _client;
        private readonly AppConfig _cfg;
        private readonly ILogger<YAGptClient> _logger;
        private readonly string _url = "https://llm.api.cloud.yandex.net/foundationModels/v1/completion";

        private readonly Regex _pattrn = MyRegex();

        public YAGptClient(HttpClient client, AppConfig cfg, ILogger<YAGptClient> logger)
        {
            _client = client;
            _cfg = cfg;
            _logger = logger;
        }
        public async Task<string> GetCategoryAsync(string text)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(_url) };
            dynamic started = new
            {
                modelUri = "gpt://b1g0selaopjmledj73k3/yandexgpt",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.1,
                    maxTokens = 1000
                },
                messages = new Message[]
                {
                        new()
                        {
                            Role = "system",
                            Text = "Тебе дается массив карточек, карточка содержит сам вопрос и ответы на него. " +
                            "Определи категорию к которым относятся карточки из данных категорий. " +
                            "Если ни одна из данных не подходит верни Разное" +
                            "Ответ должен быть в виде: «Навзание категории». Ничего лишнего не пиши. Вот пример ответ: «История»" +
                            ".1. Общекультурные знания: i. История ii. География iii. Культура и искусство iv. Литература 2. Наука и технологии: i. Физика ii. Химия iii. Биология iv. Технологии 3. Развлечения: i. Кино и телевидение ii. Музыка iii. Игры iv. Литература (художественная, фантастика и т.д.) 4. Спорт: i. Футбол ii. Баскетбол iii. Олимпийские виды iv. Экстримальные виды спорта 5. Текущие события: i. Политика ii. Новости iii. Актуальные события 6. Общество и отношения: i  Психология ii. Межличностные отношения iii. Общественные явления 7. Природа и экология: i. Экосистемы ii. Охрана окружающей среды iii. Животный мир 8. Технические знания: i. Информационные технологии ii. Программирование iii. Электроника 9.Кулинария и еда:i. Мировая кухня ii. Гастрономия iii Кулинарные традиции 10. Мировые культуры: i. Традиции различных народов ii. Религии iii. Этнография"
                        },
                        new()
                        {
                            Role =  "user",
                            Text = text
                        }
                }
            };      
            request.Headers.Add("x-folder-id", _cfg.catalogId);
            request.Headers.Add("Authorization", $"Api-Key {_cfg.YandexIAMKey}");
            request.Content = JsonContent.Create(started, new MediaTypeHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            var response = await _client.SendAsync(request);
            if(!response.IsSuccessStatusCode)
                throw new Exception("Посторонний сервис не смог выполнить задание");
            var content = await response.Content.ReadFromJsonAsync<YAGptResponse>().ConfigureAwait(false);
            YAGptResult? result = content?.Result;

            if (result is null)
                throw new ArgumentNullException("Пришел пустой ответ");

            var dirtyCategory =  result.Alternatives.FirstOrDefault()?.Message.Text ?? throw new ArgumentNullException();
            var match = _pattrn.Match(dirtyCategory);

            _logger.LogInformation($"Ответ от сервиса YandexGPT был {dirtyCategory}");

            if (!match.Success)
                throw new ArgumentException("Пришел некорректный ответ");
            return match.Groups[1].Value.Trim('«', '»');
        }

        [GeneratedRegex("«([^»]+)»")]
        private static partial Regex MyRegex();
    }
}


