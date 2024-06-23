using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using RedditDemoAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IMessageQueue, MessageQueue>();

builder.Services.AddServerSentEvents();

builder.Services.AddHostedService<RedditWorker>();

builder.Services.AddSingleton<RedditStatsService>();
builder.Services.AddSingleton<IRedditStatsProducer>(x => x.GetRequiredService<RedditStatsService>());
builder.Services.AddSingleton<IRedditStatsReader>(x => x.GetRequiredService<RedditStatsService>());

builder.Services.AddResponseCompression(options =>
{
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/event-stream" });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TheCodeBuzzService", Version = "v1" });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://www.reddit.com/api/v1/authorize", UriKind.Absolute),
                TokenUrl = new Uri("https://www.reddit.com/api/v1/access_token", UriKind.Absolute),
                RefreshUrl = new Uri("https://www.reddit.com/api/v1/access_token", UriKind.Absolute),
                Scopes = new Dictionary<string, string>
                            {
                                { "read", "Access read operations" }
                            }
            }
        }
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
              new OpenApiSecurityScheme {
                    Reference = new OpenApiReference {
                          Id = "OAuth",
                          Type = ReferenceType.SecurityScheme
                    }
              },
              new List<string>()
        }
    }
);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Version 1.0");
        c.OAuthClientId("client-id");
        c.OAuthClientSecret("client-secret");
        c.OAuthRealm("client-realm");
        c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "duration", "permanent" } });
        c.OAuthAppName("OAuth-app");
        c.EnablePersistAuthorization();
        c.OAuth2RedirectUrl("http://localhost:5095/redditauthentication");
    });
}

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();

//app.MapServerSentEvents("/rn-updates");

app.Run();
