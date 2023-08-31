using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHtmlDocument
{
    KdmidStart GetStart(string page);
    string GetApplicationFormData(string page);
    KdmidCalendar GetCalendar(string page);
    KdmidConfirmation GetConfirmation(string page);
}
