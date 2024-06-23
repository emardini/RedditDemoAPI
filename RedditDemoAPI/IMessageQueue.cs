namespace RedditDemoAPI;

public interface IMessageQueue
{
    void Register(string id);
    void Unregister(string id);
    IAsyncEnumerable<string> DequeueAsync(string id, CancellationToken cancelToken);
    Task EnqueueAsync(string id, string message, CancellationToken cancelToken);

}
