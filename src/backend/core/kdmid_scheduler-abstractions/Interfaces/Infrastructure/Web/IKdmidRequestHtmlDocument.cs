using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;

public interface IKdmidRequestHtmlDocument
{
    StartPage GetStartPage(string payload);
    string GetApplicationFormData(string payload);
    CalendarPage GetCalendarPage(string payload);
    ConfirmationPage GetConfirmationPage(string payload);
}
