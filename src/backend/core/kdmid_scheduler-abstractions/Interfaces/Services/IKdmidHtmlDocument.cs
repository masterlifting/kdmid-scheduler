using KdmidScheduler.Abstractions.Models.v1;

namespace KdmidScheduler.Abstractions.Interfaces.Services;

public interface IKdmidHtmlDocument
{
    StartPage GetStartPage(string payload);
    string GetApplicationFormData(string payload);
    CalendarPage GetCalendarPage(string payload);
    ConfirmationPage GetConfirmationPage(string payload);
}
