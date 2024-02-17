namespace quiz_web_app.Services
{
    public interface IRepository<T>
    {
        Task<T> AddAsync(T entity);
        void DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAsync(string sortParam, string sortOrder, int page, int pageSize, string filter);
        Task<T> GetByIdAsync(Guid id);
    }
}
