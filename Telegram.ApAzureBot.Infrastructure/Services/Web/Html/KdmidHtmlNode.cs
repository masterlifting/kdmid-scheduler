using HtmlAgilityPack;

using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Html;

public sealed class KdmidHtmlNode : IHtmlNode
{
    private readonly HtmlNode? _node;
    public KdmidHtmlNode(HtmlNode? node)
    {
        _node = node;
        Name = node?.Name ?? string.Empty;
        InnerText = node?.InnerText ?? string.Empty;
        
        if(node?.NextSibling is not null)
            NextSibling = new KdmidHtmlNode(node.NextSibling);
        else
            NextSibling = null;
        
        if(node?.ChildNodes is not null)
            ChildNodes = node.ChildNodes.Select(x => new KdmidHtmlNode(x));
    }

    public string Name { get; }
    public string InnerText { get; }
    public IHtmlNode? NextSibling { get; }
    public IEnumerable<IHtmlNode> ChildNodes { get; } = Enumerable.Empty<IHtmlNode>();

    public string GetAttributeValue(string name) => _node?.GetAttributeValue(name, "") ?? string.Empty;
    public IEnumerable<IHtmlNode> SelectNodes(string xpath) => _node?.SelectNodes(xpath)?.Select(x => new KdmidHtmlNode(x)) ?? Enumerable.Empty<IHtmlNode>();
}
