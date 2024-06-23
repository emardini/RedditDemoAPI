using Microsoft.AspNetCore.Mvc;
using RedditDemoAPI.Core;

namespace RedditDemoAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class SubredditStatsController : ControllerBase
{
    private readonly IRedditStatsReader redditStatsReader;

    public SubredditStatsController(IRedditStatsReader redditStatsReader)
    {
        this.redditStatsReader = redditStatsReader ?? throw new ArgumentNullException(nameof(redditStatsReader));
    }

    /// <summary>
    /// Returns a list of users sorted by users with most posts
    /// It also takes into account when the user deletes the post to also decrease the post counter
    /// </summary>
    /// <param name="percentage">Percentage of top N results to display</param>
    /// <returns>A list of users with number of psots sorted by the number of posts descendent</returns>
    [HttpGet("/UserStats/{percentage:range(1,100)}", Name = "GetSubredditsUserStats")]
    public ActionResult<IEnumerable<UserStats>> GetSubredditsUserStats(int percentage = 10)
    {
        return this.Ok(redditStatsReader.GetTopUserStatsByPercentage(percentage));
    }

    /// <summary>
    /// Returns a list of posts sorted by most upvotes
    /// It also tracks down the score (up minus down votes) and the down votes
    /// </summary>
    /// <param name="percentage">Percentage of top N results to display</param>
    /// <returns>A list of posts with number of posts sorted by the number of upvotes descendent</returns>
    [HttpGet("/PostStats/{percentage:range(1,100)}", Name = "GetSubredditsPostStats")]
    public ActionResult<IEnumerable<PostStats>> GetSubredditsPostStats(int percentage = 10)
    {
        return this.Ok(redditStatsReader.GetTopPostStatsByPercentage(percentage));
    }

}

