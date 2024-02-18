namespace quiz_web_app.Services.Repositories
{
    public interface IRepository<T>
    {
        Task<T> AddAsync(T entity);
        void DeleteAsync(T entity);
        Task<List<T>> GetAsync(string sortParam, string sortOrder, int page, int pageSize, string filter);
        Task<T> GetByIdAsync(Guid id);
    }
}
