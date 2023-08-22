namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidCaptchaService
{
    Task<string> SolveInteger(string captchaUrl, CancellationToken cToken);
}
