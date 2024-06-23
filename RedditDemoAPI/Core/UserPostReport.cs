namespace RedditDemoAPI.Core;

public class UserPostReport
{
    public required string Author { get; set; }
    public required string Title { get; set; }
    public required int NbPosts { get; set; }
    public required DateTime AddedTimestamp { get; set; }
}