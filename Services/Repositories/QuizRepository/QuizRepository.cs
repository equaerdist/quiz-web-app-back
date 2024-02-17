using Core.Models;
using Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using quiz_web_app.Data;
using quiz_web_app.Infrastructure;
using quiz_web_app.Infrastructure.Exceptions;
using quiz_web_app.Models;

namespace quiz_web_app.Services.Repositories.QuizRepository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizAppContext _ctx;
        private readonly IDistributedCache _cache;
        private readonly AppConfig _cfg;

        public QuizRepository(QuizAppContext ctx, IDistributedCache cache,
            AppConfig cfg)
        {
            _ctx = ctx;
            _cache = cache;
            _cfg = cfg;
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
               .Include(q => q.QuizCards)
               .ToListAsync();

            return quizes;
        }

        public async Task<Quiz> GetByIdAsync(Guid id)
        {
            var cacheKey = $"{_cfg.QuizCachePrefix}{id.ToString()}";
            var quizDbString = await _cache.GetStringAsync(cacheKey);
            if(quizDbString is not null)
                return JsonConvert.DeserializeObject<Quiz>(quizDbString)!;
            var quizDb = await _ctx.Quizes
                .Include(q => q.QuizCards)
                .FirstOrDefaultAsync(q => q.Id == id && q.Mode == AccessType.Public)
                .ConfigureAwait(false);
            if (quizDb is null)
                throw new BaseQuizAppException("Такого квиза не существует");
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(quizDb));
            return quizDb;
        }
    }
}
