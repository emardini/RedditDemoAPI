namespace RedditDemoAPI.Core;

public class UserStats
{
    public required string Author { get; set; }
    public required int NbPosts { get; set; }
    public required DateTime AddedTimestamp { get; set; }
}
