using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(KdmidCity city, string parameters, CancellationToken cToken);
    Task<byte[]> GetCaptchaImage(long chatId, KdmidCity city, string parameters, CancellationToken cToken);
    Task<string> PostStartPageResult(long chatId, KdmidCity city, string parameters, string data, CancellationToken cToken);
    Task<string> PostConfirmPageResult(KdmidCity city, string parameters, string data, CancellationToken cToken);
    Task<string> PostConfirmPageResult(long chatId, KdmidCity city, string parameters, string data, CancellationToken cToken);
}
