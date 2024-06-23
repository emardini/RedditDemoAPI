using System.Runtime.Serialization;

namespace RedditDemoAPI.Core;

public class RedditDemoException : Exception
{
    public RedditDemoException()
    {
    }

    public RedditDemoException(string? message) : base(message)
    {
    }

    public RedditDemoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected RedditDemoException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
