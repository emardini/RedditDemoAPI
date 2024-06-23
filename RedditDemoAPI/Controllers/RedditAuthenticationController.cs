using Microsoft.AspNetCore.Mvc;

namespace RedditDemoAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("[controller]")]
public class RedditAuthenticationController : ControllerBase
{
    private readonly ILogger<RedditAuthenticationController> _logger;

    public RedditAuthenticationController(ILogger<RedditAuthenticationController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetAutorizationCode")]
    public IActionResult Get(string state, string code)
    {
        return this.Ok(new { Code = code, Sate = state });
    }
}
