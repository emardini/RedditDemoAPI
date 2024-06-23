using System.Collections.Concurrent;
using System.Threading.Channels;

namespace RedditDemoAPI;


public class MessageQueue : IMessageQueue
{
    private ConcurrentDictionary<string, Channel<string>> clientToChannelMap;
    public MessageQueue()
    {
        clientToChannelMap = new ConcurrentDictionary<string, Channel<string>>();
    }

    public IAsyncEnumerable<string> DequeueAsync(string id, CancellationToken cancelToken)
    {
        if (clientToChannelMap.TryGetValue(id, out var channel))
        {
            return channel.Reader.ReadAllAsync(cancelToken);
        }
        else
        {
            throw new ArgumentException($"Id {id} isn't registered");
        }
    }

    public async Task EnqueueAsync(string id, string message, CancellationToken cancelToken)
    {
        if (clientToChannelMap.TryGetValue(id, out var channel))
        {
            await channel.Writer.WriteAsync(message, cancelToken);
        }
    }

    public void Register(string id)
    {
        if (!clientToChannelMap.TryAdd(id, Channel.CreateUnbounded<string>()))
        {
            throw new ArgumentException($"Id {id} is already registered");
        }
    }

    public void Unregister(string id)
    {
        clientToChannelMap.TryRemove(id, out _);
    }

    //private Channel<string> CreateChannel()
    //{
    //    return Channel.CreateUnbounded<string>();
    //}
}
