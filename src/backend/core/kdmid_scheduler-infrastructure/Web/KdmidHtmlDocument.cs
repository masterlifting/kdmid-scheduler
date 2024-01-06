using System.Text;

using HtmlAgilityPack;
using KdmidScheduler.Abstractions.Interfaces.Services;
using KdmidScheduler.Abstractions.Models.v1;

namespace KdmidScheduler.Infrastructure.Web;

public sealed class KdmidHtmlDocument : IKdmidHtmlDocument
{
    private readonly HtmlDocument _htmlDocument = new();

    public StartPage GetStartPage(string page)
    {
        _htmlDocument.LoadHtml(page);

        var pageNodes = _htmlDocument.DocumentNode.SelectNodes("//input | //img");

        if (pageNodes is null || pageNodes.Count == 0)
            throw new InvalidOperationException("The Start page was not found.");

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
                    throw new InvalidOperationException("The Captcha code was not found.");
                }
            }
        }

        formBuilder.Remove(0, 1);

        var formData = formBuilder.ToString();

        return new StartPage(formData, captchaCode);
    }
    public string GetApplicationFormData(string page)
    {
        _htmlDocument.LoadHtml(page);

        var pageNodes = _htmlDocument.DocumentNode.SelectNodes("//input");

        if (pageNodes is null || pageNodes.Count == 0)
            throw new InvalidOperationException("The Application form data was not found.");

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
    public CalendarPage GetCalendarPage(string page)
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

        if (pageNodes is null || pageNodes.Count == 0)
            throw new InvalidOperationException("The Calendar page was not found.");

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

        var variants = new Dictionary<string, string>(22);

        foreach (var radio in resultTable.SelectNodes("//input[@type='radio']"))
        {
            var radioKey = radio.NextSibling.InnerText.Trim();
            var radioValue = radio.GetAttributeValue("value", "");

            variants.Add(radioKey, radioValue);
        }

        return new CalendarPage(formData.ToString(), variants);
    }
    public ConfirmationPage GetConfirmationPage(string page)
    {
        _htmlDocument.LoadHtml(page);

        var resultTable = _htmlDocument
            .DocumentNode
            .SelectNodes("//td[@id='center-panel']")
            .FirstOrDefault();

        if (resultTable is null)
            return new("The Confirmation page was not found.");

        var result = resultTable.ChildNodes
            .Where(x => x.Name == "div" && !string.IsNullOrWhiteSpace(x.InnerText))
            .Skip(1)
            .FirstOrDefault()
            ?.ChildNodes
            .FirstOrDefault(x => x.Name == "span")
            ?.InnerText;

        return result is null
            ? new("The Confirmation page was not recognized.")
            : new(string.Join(" ", result.Split("\n").Select(x => x.Trim())));
    }
}
