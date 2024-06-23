namespace RedditDemoAPI.Core;

public interface IRedditStatsReader
{
    public IEnumerable<UserStats> GetTopUserStatsByPercentage(int percentage = 100);
    IEnumerable<PostStats> GetTopPostStatsByPercentage(int percentage = 100);
}
