using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

namespace RedditDemoAPI;

public class RedditWorker : IHostedService
{
    private readonly ILogger<RedditWorker> _logger;
    private readonly IRedditStatsProducer redditStatsProducer;

    public RedditWorker(ILogger<RedditWorker> logger, IRedditStatsProducer redditStatsProducer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.redditStatsProducer = redditStatsProducer ?? throw new ArgumentNullException(nameof(redditStatsProducer));
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(RedditWorker)} Hosted Service running.");

        var r = new RedditClient("ApiDemo001", "98461062849579-HAvU7PxDt2a57YViiLLDODhZoIEtZA", "n1BJapPbXIqBFcdHsZE0vYd_5lPFSw", "eyJhbGciOiJSUzI1NiIsImtpZCI6IlNIQTI1NjpzS3dsMnlsV0VtMjVmcXhwTU40cWY4MXE2OWFFdWFyMnpLMUdhVGxjdWNZIiwidHlwIjoiSldUIn0.eyJzdWIiOiJ1c2VyIiwiZXhwIjoxNzE5MTk1NzU3Ljg5OTk4MywiaWF0IjoxNzE5MTA5MzU3Ljg5OTk4MywianRpIjoiZEJKTlYtaV8wek9OQUtIcmsxbVNBNS1tTUNzeC13IiwiY2lkIjoiSGhiTURqMV9EZThUTWYyajJFaU4wUSIsImxpZCI6InQyX3l3Z2R5bXE3ZiIsImFpZCI6InQyX3l3Z2R5bXE3ZiIsImxjYSI6MTcxMzg2ODM2NTc4Niwic2NwIjoiZUp5S1ZpcEtUVXhSaWdVRUFBRF9fd3ZFQXBrIiwicmNpZCI6Im5sYnAzT2M1VE8tUzJKckRxdHgwYlFDLVlFX2ljMVB2Z2FxWGxmSHhMazQiLCJmbG8iOjh9.jXlUei4qRWH-_qCPV2iHCa26bIHnl-wGVJV0a-DxYAV1pSsFprq0xqHqHx0UlWe9eqpcwwymlOluSSj7d-kNdiD4UhY8XfLXRA5NN5DRyJcWfyAc-oNWrxKqIvLo3UiygoxTMqk2XhwpPphDaEQkGK43DlK_xZxJ_qtmVlLl7nc-wg6PxAMoJBiDLsOdpnq0WifVPRd_F4Uz7_d0t_rfU1iN4LOt0ezW7bdifHPik5F4Lx3deu4cNmO1q774OOp3oM2ptOSPRCHTKtoipNNLg4MKagWfJpcPVRgboP3TAT2phtlH_mdNmnbW_lH5c0kR-yxZM13Z7KtYFvY-eNn2Iw");

        var monitoredSubReddit = r.Subreddit("AskReddit").About();

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
        e.Removed.ForEach(post => redditStatsProducer.RemovePost(new PostStats { PostId = post.Id, PostTitle = post.Title, Score = post.UpVotes - post.DownVotes, AddedTimestamp = DateTime.UtcNow }));

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
        redditStatsProducer.ReportPostsStats(new PostStats { PostId = e.NewPost.Id, PostTitle = e.NewPost.Title, Score = e.NewPost.UpVotes - e.NewPost.DownVotes, AddedTimestamp = DateTime.UtcNow });
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(RedditWorker)} Hosted Service is stopping.");

        return Task.CompletedTask;
    }
}