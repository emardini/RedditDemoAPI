using Microsoft.OpenApi.Models;
using Reddit;
using RedditDemoAPI.Core;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RedditDemoAPI;

public static class ServiceConfigurationHelper
{
    public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(x =>
        {
            // serialize enums as strings in api responses (e.g. Role)
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddHostedService<RedditWorker>();
        services.AddSingleton<RedditStatsService>();
        services.AddSingleton<IRedditStatsProducer>(x => x.GetRequiredService<RedditStatsService>());
        services.AddSingleton<IRedditStatsReader>(x => x.GetRequiredService<RedditStatsService>());
        services.AddSingleton(x => {
            var options = configuration.GetSection("RedditClient").Get<RedditClientOptions>() ?? throw new RedditDemoException("Unable to obtain RedditClient credentials from configuration");
            var client = new RedditClient(options.AppId, options.RefreshToken, options.AppSecret, options.AccessToken, options.UserAgent);

            return client;
        }
        );         

        return services;
    }

    public static IServiceCollection ConfigureSwaggerGen(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "RedditAPIDemo", Version = "v1" });
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
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
            });
        });

        return services;
    }
}
