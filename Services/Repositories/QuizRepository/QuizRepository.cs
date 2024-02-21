using Core.Models;
using Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Exceptions;
using quiz_web_app.Models;
using quiz_web_app.Services.KeyResolver;

namespace quiz_web_app.Services.Repositories.QuizRepository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly AppConfig _cfg;
        private readonly IKeyResolver _keyResolver;

        public QuizRepository(QuizAppContext ctx, 
            IDistributedCache cache,
            AppConfig cfg,
            IKeyResolver keyResolver)
        {
            _ctx = ctx;
            _cache = cache;
            _cfg = cfg;
            _keyResolver = keyResolver;
        }
        public Task<Quiz> AddAsync(Quiz entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteAsync(Quiz entity)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Quiz>> GetAsync(string sortParam, string sortOrder, int page, int pageSize, string filter)
        {
            var quizes = await _ctx.Quizes
               .Where(t => t.Mode == AccessType.Public)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();

            return quizes;
        }

        public async Task<Quiz> GetByIdAsync(Guid id)
        {
            var cacheKey = _keyResolver.GetQuizKey(id);
            var quizDbString = await _cache.GetStringAsync(cacheKey);
            if (quizDbString is not null)
                return JsonConvert.DeserializeObject<Quiz>(quizDbString)!;
            var quizDb = await _ctx.Quizes
                .Include(q => q.QuizCards)
                .ThenInclude(c => c.Questions)
                .Include(q => q.QuizCards)
                .ThenInclude(c => c.QuestionsRelationships)
                .FirstOrDefaultAsync(q => q.Id == id && q.Mode == AccessType.Public)
                .ConfigureAwait(false);
            if (quizDb is null)
                throw new BaseQuizAppException("Такого квиза не существует");
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(quizDb));
            return quizDb;
        }
    }
}
