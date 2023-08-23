namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(string url, CancellationToken cToken);
    Task<string> PostStartPageResult(Uri uri, string data, CancellationToken cToken);
    Task PostConfirmPage(string url, string data, CancellationToken cToken);
    Task<string> GetConfirmPageResult(string url, string data, CancellationToken cToken);
    Task<string> GetConfirmCalendar(string url, CancellationToken cToken);
}
