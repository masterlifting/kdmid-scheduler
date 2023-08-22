using System.Text;

using HtmlAgilityPack;

using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Infrastructure.Exceptions;

namespace Telegram.ApAzureBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidHtmlDocument : IKdmidHtmlDocument
{
    private HtmlDocument _htmlDocument;
    public KdmidHtmlDocument() => _htmlDocument = new HtmlDocument();

    public KdmidStartPage GetStartPage(string page)
    {
        _htmlDocument.LoadHtml(page);

        var pageNodes = _htmlDocument.DocumentNode.SelectNodes("//input | //img");

        if (pageNodes is null || !pageNodes.Any())
            throw new ApAzureBotInfrastructureException("Page data was not found.");

        var captchaCode = string.Empty;

        StringBuilder formBuilder = new();

        foreach (var node in pageNodes)
        {
            if (node.Name == "input")
            {
                var inputName = node.GetAttributeValue("name", "");
                var inputValue = node.GetAttributeValue("value", "");

                var encodedInputName = Uri.EscapeDataString(inputName);
                var encodedInputValue = Uri.EscapeDataString(inputValue);

                formBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }
            else if (node.Name == "img")
            {
                captchaCode = node.GetAttributeValue("src", "");

                if (string.IsNullOrWhiteSpace(captchaCode) || !captchaCode.Contains("CodeImage", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ApAzureBotInfrastructureException("Captcha code was not found.");
                }
            }
        }

        formBuilder.Remove(0, 1);

        var formData = formBuilder.ToString();

        return new KdmidStartPage(formData, captchaCode);
    }
    public string GetStartPageResultFormData(string page)
    {
        _htmlDocument.LoadHtml(page);

        var pageNodes = _htmlDocument.DocumentNode.SelectNodes("//input");

        if (pageNodes is null || !pageNodes.Any())
            throw new ApAzureBotInfrastructureException("Page data was not found.");

        StringBuilder formBuilder = new();

        foreach (var node in pageNodes)
        {
            var inputName = node.GetAttributeValue("name", "");
            var inputValue = node.GetAttributeValue("value", "");

            var encodedInputName = Uri.EscapeDataString(inputName);
            var encodedInputValue = Uri.EscapeDataString(inputValue);

            if (!encodedInputName.Equals("ctl00%24MainContent%24ButtonB", StringComparison.OrdinalIgnoreCase))
            {
                formBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }
            else
            {
                formBuilder.Append($"&{encodedInputName}.x=100");
                formBuilder.Append($"&{encodedInputName}.y=20");
            }
        }

        formBuilder.Remove(0, 1);

        return formBuilder.ToString();
    }
    public KdmidConfirmPage GetConfirmPage(string page)
    {
        _htmlDocument.LoadHtml(page);

        var resultTable = _htmlDocument
            .DocumentNode
            .SelectSingleNode("//td[@id='center-panel']")
            .ChildNodes
            .FirstOrDefault(x => x.Name == "table");

        if (resultTable is null)
            return new(string.Empty, new Dictionary<string, string>(0));

        var pageNodes = _htmlDocument.DocumentNode.SelectNodes("//input");

        if (pageNodes is null || !pageNodes.Any())
            throw new ApAzureBotInfrastructureException("Page data was not found.");

        var formData = new StringBuilder();

        foreach (var node in pageNodes)
        {
            var inputName = node.GetAttributeValue("name", "");
            var inputValue = node.GetAttributeValue("value", "");

            var encodedInputName = Uri.EscapeDataString(inputName);
            var encodedInputValue = Uri.EscapeDataString(inputValue);

            formData.Append($"&{encodedInputName}={encodedInputValue}");
        }

        formData.Remove(0, 1);

        var scheduling = new Dictionary<string, string>(22);

        foreach (var radio in resultTable.SelectNodes("//input[@type='radio']"))
        {
            var radioKey = radio.NextSibling.InnerText.Trim();
            var radioValue = radio.GetAttributeValue("value", "");

            scheduling.Add(radioKey, radioValue);
        }

        return new KdmidConfirmPage(formData.ToString(), scheduling);
    }
}
