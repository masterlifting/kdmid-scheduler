using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;

public interface IKdmidRequestHttpClient
{
    Task<string> GetStartPage(City city, KdmidId kdmidId, CancellationToken cToken);
    Task<byte[]> GetStartPageCaptchaImage(City city, KdmidId kdmidId, string captchaCode, CancellationToken cToken);
    Task<string> PostApplication(City city, KdmidId kdmidId, string content, CancellationToken cToken);
    Task<string> PostCalendar(City city, KdmidId kdmidId, string content, CancellationToken cToken);
    Task<string> PostConfirmation(City city, KdmidId kdmidId, string content, CancellationToken cToken);
}
