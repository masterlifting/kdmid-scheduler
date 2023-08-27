namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidCaptchaService
{
    Task<string> SolveInteger(byte[] img, CancellationToken cToken);
}
