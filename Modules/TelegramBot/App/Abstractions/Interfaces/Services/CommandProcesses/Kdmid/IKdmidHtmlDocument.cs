using TelegramBot.Abstractions.Models.CommandProcesses.Kdmid;

namespace TelegramBot.Abstractions.Interfaces.Services.CommandProcesses.Kdmid;

public interface IKdmidHtmlDocument
{
    KdmidStart GetStart(string page);
    string GetApplicationFormData(string page);
    KdmidCalendar GetCalendar(string page);
    KdmidConfirmation GetConfirmation(string page);
}
