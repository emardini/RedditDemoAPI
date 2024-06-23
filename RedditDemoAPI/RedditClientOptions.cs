namespace RedditDemoAPI;

public class RedditClientOptions
{
    public required string AppId { get; set; }
    public required string RefreshToken { get; set; }
    public required string AppSecret { get; set; }
    public required string AccessToken { get; set; }
    public required string UserAgent { get; set; }
}
