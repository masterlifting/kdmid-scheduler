using HtmlAgilityPack;

using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Html;

public sealed class KdmidHtmlNode : IHtmlNode
{
    private readonly HtmlNode _node;
    public KdmidHtmlNode(HtmlNode node)
    {
        _node = node;
        Name = node.Name;
        InnerText = node.InnerText;
        NextSibling = new KdmidHtmlNode(node.NextSibling);
        ChildNodes = node.ChildNodes.Select(x => new KdmidHtmlNode(x));
    }

    public string Name { get; }
    public string InnerText { get; }
    public IHtmlNode NextSibling { get; }
    public IEnumerable<IHtmlNode> ChildNodes { get; }

    public string GetAttributeValue(string name) => _node.GetAttributeValue(name, "");
    public IEnumerable<IHtmlNode> SelectNodes(string xpath) => _node.SelectNodes(xpath).Select(x => new KdmidHtmlNode(x));
}
