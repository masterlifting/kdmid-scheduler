namespace Telegram.ApAzureBot.Services.Interfaces;

public interface IMidRfService : IProcessService
{
    Task Schedule(long chatId, string parameters, CancellationToken cToken);
    Task Captcha(long chatId, string parameters, CancellationToken cToken);
    Task Confirm(long chatId, string parameters, CancellationToken cToken);
}
