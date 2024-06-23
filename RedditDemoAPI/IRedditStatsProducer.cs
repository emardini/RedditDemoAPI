


namespace RedditDemoAPI;

public interface IRedditStatsProducer
{
    public void ReportUsersStats(UserPostReport stats);
    public void ReportPostsStats(PostStats stats);
    public void RemovePost(PostStats stats);
}
