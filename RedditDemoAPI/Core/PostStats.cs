﻿namespace RedditDemoAPI.Core;

public class PostStats
{
    public string PostId { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public int Score { get { return UpVotes - DownVotes; }  }
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public required DateTime AddedTimestamp { get; set; }
    public virtual Status Status { get; protected set; } = Status.Active;
}


