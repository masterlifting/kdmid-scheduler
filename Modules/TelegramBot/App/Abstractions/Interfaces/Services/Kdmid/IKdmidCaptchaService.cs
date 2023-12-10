namespace TelegramBot.Abstractions.Interfaces.Services.Kdmid;

public interface IKdmidCaptchaService
{
    Task<string> SolveInteger(byte[] img, CancellationToken cToken);
}
