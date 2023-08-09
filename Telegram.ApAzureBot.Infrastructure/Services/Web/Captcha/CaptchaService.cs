using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Captcha;

public sealed class CaptchaService : ICaptchaService
{
    public Task<ushort> SolveInteger(byte[] img, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
