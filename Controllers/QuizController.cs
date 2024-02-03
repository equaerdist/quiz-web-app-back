using AutoMapper;
using Core.Models;
using FluentEmail.Core;
using Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure.Exceptions;
using quiz_web_app.Models;
using quiz_web_app.Services.IYAGpt;
using System.IO;
namespace quiz_web_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : Controller
    {
        private readonly QuizAppContext _ctx;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly IYAGpt _gpt;

        public QuizController(QuizAppContext ctx, IMapper mapper, IWebHostEnvironment env, IYAGpt gpt) 
        {
            _ctx = ctx;
            _mapper = mapper;
            _env = env;
            _gpt = gpt;
        }
        [HttpGet]
        public async Task<IEnumerable<GetQuizDto>> GetQuizes(int page, int pageSize, string sortParam,  string sortOrder)
        {
            var quizes = await _ctx.Quizes
                .Where(t => t.Mode == AccessType.Public)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var result = _mapper.Map<List<GetQuizDto>>(quizes);
            for(var i=0; i < quizes.Count; i++)
            {
                var quiz = quizes[i];
                var completedQuizes = _ctx.CompletedQuizes.Where(c => c.QuizId == quiz.Id && c.Raiting != null).DistinctBy(u => u.UserId);
                result[i].Raiting = await completedQuizes.AverageAsync(c => c.Raiting) ?? 0;
            }
            return result;
        }
        [HttpGet("{id:guid}", Name = nameof(GetQuiz))]
        public async Task<GetQuizDto> GetQuiz(Guid id)
        {
            var quizDb = await _ctx.Quizes.FirstOrDefaultAsync(q => q.Id == id && q.Mode == AccessType.Public).ConfigureAwait(false);
            if (quizDb is null)
                throw new BaseQuizAppException("Такого квиза не существует");
            var result = _mapper.Map<GetQuizDto>(quizDb);
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> CreateQuiz(CreateQuizDto dto)
        {
            if (dto.Cards is null || dto.Cards.Count == 0)
                throw new BaseQuizAppException("Карточки для квиза должны существовать");

            foreach (var card in dto.Cards)
            {
                if (card.Questions is null || card.Questions.Count == 0)
                    throw new BaseQuizAppException("Вопросы для каждой карточки должны существовать");
                var rightAnswers = card.Questions.Count(q => q.Type == true);
                if (rightAnswers == 0)
                    throw new BaseQuizAppException("На каждый вопрос должен быть как минимум 1 правильный ответ");
            }
            string imgPath = dto.Thumbnail ?? "default.png";
            var cardsListDb = new List<QuizCard>();

            foreach(var card in dto.Cards)
            {
                string coverImgCard = card.Thumbnail ?? "default.png";
                var questionListDb = new List<dynamic>();
                if(card.Questions is null)
                    throw new ArgumentNullException();

                foreach(var question in card.Questions)
                {
                    string coverImgQuestion = question.Thumbnail ?? "default.png";
                    var questionDb = _mapper.Map<QuizQuestion>(question);
                    questionDb.Thumbnail = coverImgQuestion;
                    questionListDb.Add(new {QuestionDb = questionDb, Type = question.Type});
                }
                var cardDb = _mapper.Map<QuizCard>(card);
                cardDb.Thumbnail = coverImgCard;
                cardDb.QuestionsRelationships = questionListDb.Select(q => new QuizQuestionRelation() 
                                                                    { Card = cardDb, Question = q.QuestionDb, Type = q.Type }
                                                                ).ToList();
                cardsListDb.Add(cardDb);
            }

            var userDb = await _ctx.Users.FirstOrDefaultAsync();
            var quizDb = _mapper.Map<Quiz>(dto);
            quizDb.Thumbnail = imgPath;
            quizDb.QuizCards = cardsListDb;
            quizDb.QuestionsAmount = cardsListDb.Count;
            quizDb.Category = await _gpt.GetCategoryAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new { Cards = dto.Cards}));
            quizDb.Creator = userDb;
            quizDb.Mode = (AccessType)dto.Mode;

            await _ctx.Quizes.AddAsync(quizDb);
            await _ctx.SaveChangesAsync();

            var quizReadDto = _mapper.Map<GetQuizDto>(quizDb);
            return CreatedAtRoute(nameof(GetQuiz), new { id = quizDb.Id }, quizReadDto);
        }
    }
}
