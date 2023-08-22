namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidCaptchaService
{
    Task<uint> SolveInteger(string captchaUrl, CancellationToken cToken);
}
