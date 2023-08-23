namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(string url, CancellationToken cToken);
    Task<byte[]> GetCaptchaImage(long chatId, string url, CancellationToken cToken);
    Task<string> PostStartPageResult(long chatId, Uri uri, string data, CancellationToken cToken);
    Task PostConfirmPage(long chatId, string url, string data, CancellationToken cToken);
    Task<string> GetConfirmPageResult(long chatId, string url, string data, CancellationToken cToken);
    Task<string> GetConfirmCalendar(long chatId, string url, CancellationToken cToken);
}
