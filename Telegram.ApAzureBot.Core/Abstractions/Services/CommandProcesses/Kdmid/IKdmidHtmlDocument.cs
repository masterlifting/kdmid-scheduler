using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHtmlDocument
{
    KdmidConfirmPage GetCalendarPage(string page);
    KdmidStartPage GetStartPage(string page);
    string GetStartPageResultFormData(string page);
}
