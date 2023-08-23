using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHtmlDocument
{
    KdmidStartPage GetStartPage(string page);
    string GetStartPageResultFormData(string page);
    KdmidConfirmPage GetConfirmPage(string page);
}
