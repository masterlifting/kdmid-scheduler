using System.Text;

using HtmlAgilityPack;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class KdmidService : IKdmidService
{
    private static string GetIdentifierKey(string city) => $"{Constants.Kdmid}.{city}.identifier";
    private static string GetRequestKey(string city) => $"{Constants.Kdmid}.{city}.request";
    private static string GetConfirmKey(string city, string key) => $"{Constants.Kdmid}.{city}.confirm.{key}";
    private static string GetResultKey(string city) => $"{Constants.Kdmid}.{city}.result";

    private readonly MemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ITelegramService _telegramService;

    private readonly Dictionary<string, Func<long, string, string, CancellationToken, Task>> _functions;

    public KdmidService(MemoryCache cache, IHttpClientFactory httpClientFactory, ITelegramService telegramService)
    {
        _cache = cache;
        _telegramService = telegramService;

        _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);

        _functions = new()
        {
            { "schedule", Schedule },
            { "captcha", Captcha },
            { "confirm", Confirm },
        };
    }

    public Task Process(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
    {
        if (message.Length == 0)
            throw new NotSupportedException("The command is not found.");

        var cityIndex = message.IndexOf('/');

        if (cityIndex < 0)
            throw new NotSupportedException("The city for the command is not found.");

        var city = message[0..cityIndex];

        message = message[(cityIndex + 1)..];

        var nextCommandStartIndex = message.IndexOf('?');

        var command = nextCommandStartIndex > 0
            ? message[0..nextCommandStartIndex]
            : message;

        var parameters = nextCommandStartIndex > 0
            ? message[(nextCommandStartIndex + 1)..]
            : string.Empty;

        return !_functions.TryGetValue(command.ToString(), out var function)
            ? throw new NotSupportedException("The process of the command is not supported.")
            : function(chatId, city.ToString(), parameters.ToString(), cToken);
    }

    public async Task Schedule(long chatId, string city, string parameters, CancellationToken cToken)
    {
        /*/
        var url = _httpClient.BaseAddress + "OrderInfo.aspx?" + parameters;
        var page = await _httpClient.GetStringAsync(url, cToken);
        /*/
        var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/firstResponse.html");
        //*/

        _cache.AddOrUpdate(chatId, GetIdentifierKey(city), parameters);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(page);

        string? captchaUrl = null;

        StringBuilder requestFormBuilder = new();

        foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input | //img"))
        {
            if (node.Name == "input")
            {
                string inputName = node.GetAttributeValue("name", "");
                string inputValue = node.GetAttributeValue("value", "");

                string encodedInputName = Uri.EscapeDataString(inputName);
                string encodedInputValue = Uri.EscapeDataString(inputValue);

                requestFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }
            else if (node.Name == "img")
            {
                string captchaUrlPart = node.GetAttributeValue("src", "");

                if (captchaUrlPart.Contains("CodeImage", StringComparison.OrdinalIgnoreCase))
                {
                    captchaUrl = _httpClient.BaseAddress + captchaUrlPart;
                }
            }
        }
        requestFormBuilder.Remove(0, 1);

        var requestForm = requestFormBuilder.ToString();

        _cache.AddOrUpdate(chatId, GetRequestKey(city), requestForm);

        if (captchaUrl is null)
            throw new NotSupportedException("The captcha url is null.");
        else
        {
            /*/
            var captcha = await _httpClient.GetByteArrayAsync(captchaUrl, cToken);
            /*/
            var captcha = File.ReadAllBytes(Environment.CurrentDirectory + "/Content/CodeImage.jpeg");
            //*/

            using var stream = new MemoryStream(captcha);

            var photo = Bot.Types.InputFile.FromStream(stream, "captcha.jpeg");

            await _telegramService.Client.SendPhotoAsync(chatId, photo, caption: $"/{Constants.Kdmid}/{city}/captcha?number", cancellationToken: cToken);
        }
    }
    public async Task Captcha(long chatId, string city, string parameters, CancellationToken cToken)
    {
        if (parameters.Length < 6)
            throw new NotSupportedException("The captcha is not valid.");

        if (!_cache.TryGetValue(chatId, GetIdentifierKey(city), out var requestIdentifier))
            throw new NotSupportedException("The identifier is not found. Try from the beginning.");

        if (!_cache.TryGetValue(chatId, GetRequestKey(city), out var requestForm))
            throw new NotSupportedException("The request form model is not found. Try from the beginning.");

        var searchString = "ctl00%24MainContent%24txtCode=";
        var replacementString = $"ctl00%24MainContent%24txtCode={parameters}";

        var stringContent = requestForm!.Replace(searchString, replacementString);
        var content = new StringContent(stringContent, Encoding.UTF8, "application/x-www-form-urlencoded");

        var url = _httpClient.BaseAddress + "OrderInfo.aspx?" + requestIdentifier;

        /*/
        var requestResponse = await _httpClient.PostAsync(url, content, cToken);
        var requestPage = await requestResponse.Content.ReadAsStringAsync(cToken);
        /*/
        var requestPage = File.ReadAllText(Environment.CurrentDirectory + "/Content/secondResponse.html");
        //*/

        var htmlDocument = new HtmlDocument();

        htmlDocument.LoadHtml(requestPage);

        StringBuilder confirmFormBuilder = new();

        foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input"))
        {
            string inputName = node.GetAttributeValue("name", "");
            string inputValue = node.GetAttributeValue("value", "");

            string encodedInputName = Uri.EscapeDataString(inputName);
            string encodedInputValue = Uri.EscapeDataString(inputValue);

            if (!encodedInputName.Equals("ctl00%24MainContent%24ButtonB", StringComparison.OrdinalIgnoreCase))
            {
                confirmFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }
            else
            {
                confirmFormBuilder.Append($"&{encodedInputName}.x=100");
                confirmFormBuilder.Append($"&{encodedInputName}.y=20");
            }
        }

        confirmFormBuilder.Remove(0, 1);

        var confirmForm = confirmFormBuilder.ToString();

        content = new StringContent(confirmForm, Encoding.UTF8, "application/x-www-form-urlencoded");
        /*/
        var confirmResponse = await _httpClient.PostAsync(url, content, cToken);
        var confirmPage = await confirmResponse.Content.ReadAsStringAsync(cToken);
        /*/
        var confirmPage = File.ReadAllText(Environment.CurrentDirectory + "/Content/thirdResponse_Ok.html");
        //*/

        htmlDocument.LoadHtml(confirmPage);

        var scheduleTable = htmlDocument.DocumentNode
            .SelectSingleNode("//td[@id='center-panel']")
            .ChildNodes.FirstOrDefault(x => x.Name == "table");

        if (scheduleTable is null)
        {
            await _telegramService.Client.SendTextMessageAsync(chatId, "Making appointments are not found.", cancellationToken: cToken);
        }
        else
        {
            StringBuilder resultFormBuilder = new();

            foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input"))
            {
                string inputName = node.GetAttributeValue("name", "");
                string inputValue = node.GetAttributeValue("value", "");

                string encodedInputName = Uri.EscapeDataString(inputName);
                string encodedInputValue = Uri.EscapeDataString(inputValue);

                resultFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }

            resultFormBuilder.Remove(0, 1);

            var resultForm = resultFormBuilder.ToString();

            _cache.AddOrUpdate(chatId, GetResultKey(city), resultForm);

            await _telegramService.Client.SendTextMessageAsync(chatId, "Making appointments are found. Choose one of them:", cancellationToken: cToken);

            foreach (var item in scheduleTable.SelectNodes("//input[@type='radio']"))
            {
                var id = item.GetAttributeValue("name", "");
                var name = item.GetAttributeValue("name", "");
                var value = item.GetAttributeValue("value", "");

                var appointment = item.NextSibling.InnerText.Trim();

                _cache.AddOrUpdate(chatId, GetConfirmKey(city, appointment), value);

                await _telegramService.Client.SendTextMessageAsync(chatId, appointment, cancellationToken: cToken);
            }

            await _telegramService.Client.SendTextMessageAsync(chatId, $"/{Constants.Kdmid}/{city}/confirm?appointment", cancellationToken: cToken);
        }
    }
    public async Task Confirm(long chatId, string city, string parameters, CancellationToken cToken)
    {
        if (!_cache.TryGetValue(chatId, GetIdentifierKey(city), out var requestIdentifier))
            throw new NotSupportedException("The identifier is not found. Try from the beginning.");

        if (!_cache.TryGetValue(chatId, GetConfirmKey(city, parameters), out var confirmValue))
            throw new NotSupportedException("The appointment is not found. Try from the beginning.");

        if (!_cache.TryGetValue(chatId, GetResultKey(city), out var resultForm))
            throw new NotSupportedException("The result form model is not found. Try from the beginning.");

        var searchString = "ctl00%24MainContent%24TextBox1=";
        var replacementString = $"ctl00%24MainContent%24TextBox1={confirmValue}";

        var stringContent = resultForm!.Replace(searchString, replacementString);

        await _telegramService.Client.SendTextMessageAsync(chatId, "Confirmed.", cancellationToken: cToken);
    }
}
