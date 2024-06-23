using RedditDemoAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDependencies(builder.Configuration);

builder.Services.ConfigureSwaggerGen();

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


app.Run();
