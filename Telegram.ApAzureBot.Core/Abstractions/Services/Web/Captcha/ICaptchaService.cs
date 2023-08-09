namespace Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;

public interface ICaptchaService
{
    Task<uint> SolveInteger(byte[] img, CancellationToken cToken);
}
