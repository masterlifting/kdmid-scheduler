using KdmidScheduler.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces;

public interface IKdmidHtmlDocument
{
    StartPage GetStartPage(string payload);
    string GetApplicationFormData(string payload);
    CalendarPage GetCalendarPage(string payload);
    ConfirmationPage GetConfirmationPage(string payload);
}
