using TelegramBot.Abstractions.Models;

namespace TelegramBot.Abstractions.Interfaces.Services.Kdmid;

public interface IKdmidHtmlDocument
{
    StartPageResult GetStart(string page);
    string GetApplicationFormData(string page);
    CalendarPageResult GetCalendar(string page);
    ConfirmationPageResult GetConfirmation(string page);
}
