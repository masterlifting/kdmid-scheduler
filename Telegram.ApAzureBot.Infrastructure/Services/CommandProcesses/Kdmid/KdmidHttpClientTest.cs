using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHttpClientTest : IKdmidHttpClient
{
    public Task<string> GetStartPage(string url, CancellationToken cToken)
    {
        var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/firstResponse.html");

        return string.IsNullOrEmpty(page)
            ? throw new Exception("Start page is empty")
            : Task.FromResult(page);
    }
    public Task<string> GetStartPageResult(string url, string data, CancellationToken cToken)
    {
        var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/secondResponse.html");

        return string.IsNullOrEmpty(page)
            ? throw new Exception("Start page result is empty")
            : Task.FromResult(page);
    }

    public Task PostConfirmPage(string url, string data, CancellationToken cToken)
    {
        var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/thirdResponse_Ok.html");

        return string.IsNullOrEmpty(page)
            ? throw new Exception("Start page is empty")
            : Task.FromResult(page);
    }
    public Task<string> GetConfirmPageResult(string url, string data, CancellationToken cToken)
    {
        return Task.FromResult("Confirmed from test.");
    }

    public Task<string> GetConfirmCalendar(string calendarUrl, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
