namespace Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;

public interface ICaptchaService
{
    Task<ushort> SolveInteger(byte[] img, CancellationToken cToken);
}
