using Microsoft.AspNetCore.Mvc;
using Octokit;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello Copilot!");

// make sure you change the App Name below
string yourGitHubAppName = "sprint-planner";
string githubCopilotCompletionsUrl = 
    "https://api.githubcopilot.com/chat/completions";



app.MapPost("/agent", async (
    [FromHeader(Name = "X-GitHub-Token")] string githubToken, 
    [FromBody] Request userRequest) =>
{

var octokitClient = 
    new GitHubClient(
        new Octokit.ProductHeaderValue(yourGitHubAppName))
{
    Credentials = new Credentials(githubToken)
};
var user = await octokitClient.User.Current();
userRequest.Messages.Insert(0, new Message
{
    Role = "system",
    Content = 
        "Start every response with the user's name, " + 
        $"which is @{user.Login}"
});
userRequest.Messages.Insert(0, new Message
{
    Role = "system",
    Content = 
        "You are a helpful assistant that replies to " +
        "Based on the user message, generate a detailed user story. Ensure the user story includes:A clear and concise title.A well-defined description outlining the requirement.Properly structured information, including the role, action, and goal.Detailed acceptance criteria specifying the conditions for completion.Any additional relevant details such as dependencies or priorities"
});
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", githubToken);
userRequest.Stream = true;
var copilotLLMResponse = await httpClient.PostAsJsonAsync(
    githubCopilotCompletionsUrl, userRequest);
    var responseStream = 
    await copilotLLMResponse.Content.ReadAsStreamAsync();
return Results.Stream(responseStream, "application/json");

});

app.MapGet("/callback", () => "You may close this tab and " + 
    "return to GitHub.com (where you should refresh the page " +
    "and start a fresh chat). If you're using VS Code or " +
    "Visual Studio, return there.");

app.Run();
