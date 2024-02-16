using AutoMapper;
using Internal;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Services.IYAGpt;
using System.Net.Quic;

namespace quiz_web_app.Infrastructure.Consumers.QuizCreatedEventConsumer
{
    public class QuizCreatedEventConsumer : IConsumer<QuizCreatedEvent>
    {
        private readonly ILogger<QuizCreatedEventConsumer> _logger;
        private readonly QuizAppContext _ctx;
        private readonly IMapper _mapper;
        private readonly IYAGpt _gpt;

        public QuizCreatedEventConsumer(ILogger<QuizCreatedEventConsumer> logger, QuizAppContext ctx, IMapper mapper, IYAGpt gpt)
        {
            _logger = logger;
            _ctx = ctx;
            _mapper = mapper;
            _gpt = gpt;
        }
        public async Task Consume(ConsumeContext<QuizCreatedEvent> context)
        {
            _logger.LogInformation($"Quiz Event message received. Start to define category for quiz {context.Message.Id}");
            var quiz = await _ctx.Quizes
                .Include(q => q.QuizCards)
                .ThenInclude(qc => qc.Questions)
                .FirstOrDefaultAsync(q => q.Id == context.Message.Id);
            if (quiz is null) throw new ArgumentNullException(nameof(quiz));
            var cards = _mapper.Map<List<GetQuizCardDto>>(quiz.QuizCards);
            quiz.Category = await _gpt.GetCategoryAsync(JsonConvert.SerializeObject(cards));
            await _ctx.SaveChangesAsync();
            _logger.LogInformation($"Category for quiz {context.Message.Id} defined");
        }
    }
}
