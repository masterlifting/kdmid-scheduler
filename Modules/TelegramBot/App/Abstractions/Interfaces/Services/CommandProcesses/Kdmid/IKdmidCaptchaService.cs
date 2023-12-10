namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;

public interface IKdmidCaptchaService
{
    Task<string> SolveInteger(byte[] img, CancellationToken cToken);
}
