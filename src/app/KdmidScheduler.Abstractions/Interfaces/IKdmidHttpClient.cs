using KdmidScheduler.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(City city, Identifier identifier, CancellationToken cToken);
    Task<byte[]> GetStartPageCaptchaImage(City city, string captchaCode, CancellationToken cToken);
    Task<string> PostApplication(City city, Identifier identifier, string content, CancellationToken cToken);
    Task<string> PostCalendar(City city, Identifier identifier, string content, CancellationToken cToken);
    Task<string> PostConfirmation(City city, Identifier identifier, string content, CancellationToken cToken);
}
