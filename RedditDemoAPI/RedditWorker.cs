using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using RedditDemoAPI.Core;

namespace RedditDemoAPI;

public class RedditWorker : IHostedService
{
    private readonly ILogger<RedditWorker> logger;
    private readonly IRedditStatsProducer redditStatsProducer;
    private readonly RedditClient redditClient;

    public RedditWorker(ILogger<RedditWorker> logger, IRedditStatsProducer redditStatsProducer, RedditClient redditClient)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.redditStatsProducer = redditStatsProducer ?? throw new ArgumentNullException(nameof(redditStatsProducer));
        this.redditClient = redditClient ?? throw new ArgumentNullException(nameof(redditClient));
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(RedditWorker)} Hosted Service running.");
       
        var monitoredSubReddit = redditClient.Subreddit("AskReddit").About();

        monitoredSubReddit.Posts.MonitorNew();
        monitoredSubReddit.Posts.NewUpdated += NewOrDeletePostEventHandler;

        return Task.CompletedTask;
    }

    //This event will only be processed by one thread at a time because the system is currently monitoring only one subreddit
    private void NewOrDeletePostEventHandler(object? sender, PostsUpdateEventArgs e)
    {
        //Register the post to get score updates
        UpdatePostsListeners(e.Added, (post) => { post.PostScoreUpdated += PostScoreUpdatedEventHandler; });

        //Removing current score update listeners for deleted posts
        UpdatePostsListeners(e.Removed, (post) => { post.PostScoreUpdated -= PostScoreUpdatedEventHandler; });

        //Make sure the removed post is no longer tracked for score
        e.Removed.ForEach(post => redditStatsProducer.RemovePost(new PostStats { PostId = post.Id, PostTitle = post.Title, UpVotes = post.UpVotes, DownVotes = post.DownVotes, AddedTimestamp = DateTime.UtcNow }));

        //Reporting user stats
        e.Added.ForEach(p => redditStatsProducer.ReportUsersStats(new UserPostReport { Author = p.Author, NbPosts = 1, AddedTimestamp = DateTime.UtcNow, Title = p.Title }));
        e.Removed.ForEach(p => redditStatsProducer.ReportUsersStats(new UserPostReport { Author = p.Author, NbPosts = -1, AddedTimestamp = DateTime.UtcNow, Title = p.Title }));
    }

    private static void UpdatePostsListeners(List<Post> addedPosts, Action<Post> addOrRemoveEventAction)
    {
        addedPosts.ForEach(post =>
        {
            post.MonitorPostScore(null, null, 0, 0, null);
            addOrRemoveEventAction(post);
        });
    }

    private void PostScoreUpdatedEventHandler(object? sender, PostUpdateEventArgs e)
    {
        redditStatsProducer.ReportPostsStats(new PostStats { PostId = e.NewPost.Id, PostTitle = e.NewPost.Title, UpVotes = e.NewPost.UpVotes, DownVotes = e.NewPost.DownVotes, AddedTimestamp = DateTime.UtcNow });
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"{nameof(RedditWorker)} Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}