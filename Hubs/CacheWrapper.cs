namespace quiz_web_app.Hubs
{
    public class CacheWrapper<T>
    {
        public T Data { get; set; } = default(T);
    }
}
