namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(string url, CancellationToken cToken);
    Task<string> GetStartPageResult(string url, string data, CancellationToken cToken);
    Task<string> GetConfirmPage(string url, string data, CancellationToken cToken);
    Task<string> GetConfirmPageResult(string url, string data, CancellationToken cToken);
}
