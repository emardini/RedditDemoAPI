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

    [HttpGet("/UserStats/{percentage:range(1,100)}", Name = "GetSubredditsUserStats")]
    public ActionResult<IEnumerable<UserStats>> GetSubredditsUserStats(int percentage = 10)
    {
        return this.Ok(redditStatsReader.GetTopUserStatsByPercentage(percentage));
    }

    [HttpGet("/PostStats/{percentage:range(1,100)}", Name = "GetSubredditsPostStats")]
    public ActionResult<IEnumerable<PostStats>> GetSubredditsPostStats(int percentage = 10)
    {
        return this.Ok(redditStatsReader.GetTopPostStatsByPercentage(percentage));
    }

}

