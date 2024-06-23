

namespace RedditDemoAPI;

public interface IRedditStatsReader
{
    public IEnumerable<UserStats> GetTopUserStatsByPercentage(int percentage = 100);
    IEnumerable<PostStats> GetTopPoststatsByPercentage(int percentage = 100);
}
