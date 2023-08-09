namespace Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;

public interface IHtmlNode
{
    string Name { get; }
    string InnerText { get; }
    
    IHtmlNode NextSibling { get; }
    IEnumerable<IHtmlNode> ChildNodes { get; }
  
    string GetAttributeValue(string name);
    IEnumerable<IHtmlNode> SelectNodes(string xpath);
}
