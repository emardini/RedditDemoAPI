using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace RedditDemoAPI;

public class RedditStatsService : IRedditStatsProducer, IRedditStatsReader
{
    class InactivePostStats : PostStats
    {
        public override Status Status { get; protected set; } = Status.Active;
    }

    private readonly ILogger<RedditStatsService> logger;

    private readonly ConcurrentDictionary<string, UserStats> userStats = new ConcurrentDictionary<string, UserStats>();
    private readonly ConcurrentDictionary<string, PostStats> postStats = new ConcurrentDictionary<string, PostStats>();

    public RedditStatsService(ILogger<RedditStatsService> logger)
    {
        this.logger = logger;
    }

    public void RemovePost(PostStats stats)
    {
        var postResult = postStats.AddOrUpdate(stats.PostId,
           new InactivePostStats { PostId = stats.PostId, PostTitle = stats.PostTitle, Score = stats.Score, AddedTimestamp = stats.AddedTimestamp },
           (key, oldValue) => {
               //If the post has been deactivated by a previous thread, let's keep it Inactive but increase the score
               return new InactivePostStats { PostId = stats.PostId, PostTitle = stats.PostTitle, Score = oldValue.Score + stats.Score, AddedTimestamp = stats.AddedTimestamp };
           });

        logger.LogInformation("Post removed: {post}", JsonConvert.SerializeObject(stats));
    }

    public void ReportPostsStats(PostStats stats)
    {
        var postResult = postStats.AddOrUpdate(stats.PostId,
            new PostStats { PostId = stats.PostId, PostTitle = stats.PostTitle, Score = stats.Score, AddedTimestamp = DateTime.UtcNow },
            (key, oldValue) => {
                //If the post has been deactivated by a previous thread, let's keep it Inactive but increase the score
                return oldValue.Status == Status.Active ?
                    new PostStats { PostId = stats.PostId, PostTitle = stats.PostTitle, Score = oldValue.Score + stats.Score, AddedTimestamp = stats.AddedTimestamp } :
                    new InactivePostStats { PostId = stats.PostId, PostTitle = stats.PostTitle, Score = oldValue.Score + stats.Score, AddedTimestamp = stats.AddedTimestamp };
            });
        logger.LogInformation("Post reported: {post}", JsonConvert.SerializeObject(stats));
    }

    public void ReportUsersStats(UserPostReport stats)
    {
        var userResult = userStats.AddOrUpdate(stats.Author, 
            new UserStats { AddedTimestamp = stats.AddedTimestamp, Author = stats.Author, NbPosts = stats.NbPosts }, 
            (key, oldValue) => {
                return new UserStats { AddedTimestamp = stats.AddedTimestamp, Author = stats.Author, NbPosts = oldValue.NbPosts + stats.NbPosts };
            });
        logger.LogInformation("Post reported: {user}", JsonConvert.SerializeObject(stats));
    }

    public IEnumerable<UserStats> GetTopUserStatsByPercentage(int percentage = 10)
    {
        if (percentage <= 0 || percentage > 100) throw new ArgumentException("Percentage value should be between 1 and 100", nameof(percentage));

        var nbOfItems = userStats.Count;
        var taken = (int)Math.Ceiling(nbOfItems * 100.00 / percentage);
        return userStats.Values
            .OrderByDescending(s => s.NbPosts)
            .ThenBy(s => s.AddedTimestamp)
            .Take(taken);
    }

    public IEnumerable<PostStats> GetTopPoststatsByPercentage(int percentage = 10)
    {
        if (percentage <= 0 || percentage > 100) throw new ArgumentException("Percentage value should be between 1 and 100", nameof(percentage));

        var nbOfItems = postStats.Count;
        var taken = (int)Math.Ceiling(nbOfItems * 100.00 / percentage);
        return postStats.Values
            .Where(p =>  p.Status == Status.Active)
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.AddedTimestamp)
            .Take(taken);
    }
}