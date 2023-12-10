using TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;

public interface IKdmidHtmlDocument
{
    Start GetStart(string page);
    string GetApplicationFormData(string page);
    Calendar GetCalendar(string page);
    Confirmation GetConfirmation(string page);
}
