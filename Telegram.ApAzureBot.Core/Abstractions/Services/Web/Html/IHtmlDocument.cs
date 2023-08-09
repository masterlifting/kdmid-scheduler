namespace Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;

public interface IHtmlDocument
{
    void LoadHtml(string page);
    
    IEnumerable<IHtmlNode> SelectNodes(string xpath);
    IHtmlNode SelectSingleNode(string xpath);
}
