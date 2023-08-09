using HtmlAgilityPack;

using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Html;

public sealed class KdmidHtmlDocument : IHtmlDocument
{
    private HtmlDocument _htmlDocument;
    public void LoadHtml(string page)
    {
        _htmlDocument?.LoadHtml(page);
    }

    public IEnumerable<IHtmlNode> SelectNodes(string xpath) => 
        _htmlDocument.DocumentNode.SelectNodes(xpath).Select(x => new KdmidHtmlNode(x));

    public IHtmlNode SelectSingleNode(string xpath) => 
        new KdmidHtmlNode(_htmlDocument.DocumentNode.SelectSingleNode(xpath));
}
