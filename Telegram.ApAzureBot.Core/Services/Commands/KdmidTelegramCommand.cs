using System.Text;

using Telegram.ApAzureBot.Core.Abstractions.Services.CommandServices;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Services.Telegram;

namespace Telegram.ApAzureBot.Core.Services.CommandServices;

public sealed class KdmidTelegramCommand : IKdmidService
{
    private const string FormDataMediaType = "application/x-www-form-urlencoded";
    private static string GetCaptchaCommand(string city) => $"/{Constants.Kdmid}/{city}/captcha?";
    private static string GetConfirmCommand(string city) => $"/{Constants.Kdmid}/{city}/confirm?";
    private static string GetBaseUrl(string city) => $"https://{city}.{Constants.Kdmid}.ru/queue/";
    private static string GetRequestUrl(string city, string identifier) => GetBaseUrl(city) + "OrderInfo.aspx?" + identifier;
    private static string GetUrlIdentifierKey(string city) => $"{Constants.Kdmid}.{city}.identifier";
    private static string GetRequestFormKey(string city) => $"{Constants.Kdmid}.{city}.request";
    private static string GetResultFormKey(string city) => $"{Constants.Kdmid}.{city}.result";
    private static string GetConfirmValueKey(string city, string key) => $"{Constants.Kdmid}.{city}.confirm.{key}";

    private readonly TelegramMemoryCache _cache;
    private readonly ITelegramClient _telegramClient;
    private readonly IHttpClient _httpClient;
    private readonly IHtmlDocument _htmlDocument;
    private readonly ICaptchaService _captchaService;

    private readonly Dictionary<string, Func<long, string, string, CancellationToken, Task>> _functions;

    public KdmidTelegramCommand(
        TelegramMemoryCache cache
        , ITelegramClient telegramClient
        , IHttpClient httpClient
        , IHtmlDocument htmlDocument
        , ICaptchaService captchaService)
    {
        _cache = cache;
        _telegramClient = telegramClient;
        _httpClient = httpClient;
        _htmlDocument = htmlDocument;
        _captchaService = captchaService;

        _functions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "schedule", Schedule },
            { "captcha", Captcha },
            { "confirm", Confirm },
        };
    }

    public Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
    {
        if (message.Length == 0)
            throw new NotSupportedException("Command was not found.");

        var cityIndex = message.IndexOf('/');

        if (cityIndex < 0)
            throw new NotSupportedException($"City from the '{message}' was not found.");

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
            ? throw new NotSupportedException($"Command is not supported for '{city}'.")
            : function(chatId, city.ToString(), parameters.ToString(), cToken);
    }

    public async Task Schedule(long chatId, string city, string urlIdentifier, CancellationToken cToken)
    {
        var page = await _httpClient.GetStringAsync(GetRequestUrl(city, urlIdentifier), cToken);

        _cache.AddOrUpdate(chatId, GetUrlIdentifierKey(city), urlIdentifier);

        _htmlDocument.LoadHtml(page);

        string? captchaUrl = null;

        StringBuilder formBuilder = new();

        foreach (var node in _htmlDocument.SelectNodes("//input | //img"))
        {
            if (node.Name == "input")
            {
                var inputName = node.GetAttributeValue("name");
                var inputValue = node.GetAttributeValue("value");

                var encodedInputName = Uri.EscapeDataString(inputName);
                var encodedInputValue = Uri.EscapeDataString(inputValue);

                formBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }
            else if (node.Name == "img")
            {
                var captchaUrlPart = node.GetAttributeValue("src");

                if (captchaUrlPart.Contains("CodeImage", StringComparison.OrdinalIgnoreCase))
                {
                    captchaUrl = GetBaseUrl(city) + captchaUrlPart;
                }
            }
        }

        formBuilder.Remove(0, 1);

        var formData = formBuilder.ToString();

        _cache.AddOrUpdate(chatId, GetRequestFormKey(city), formData);

        if (captchaUrl is null)
            throw new ArgumentException($"Captcha was not found for '{city}'.");
        else
        {
            var captcha = await _httpClient.GetByteArrayAsync(captchaUrl, cToken);
            var captchaResult = await _captchaService.SolveInteger(captcha, cToken);
            await Captcha(chatId, city, captchaResult.ToString(), cToken);
        }
    }
    public async Task Captcha(long chatId, string city, string captcha, CancellationToken cToken)
    {
        if (captcha.Length < 6 || !uint.TryParse(captcha, out _))
            throw new NotSupportedException($"Captcha is not valid for '{city}'.");

        if (!_cache.TryGetValue(chatId, GetUrlIdentifierKey(city), out var urlIdentifier))
            throw new NotSupportedException($"URL identifier was not found for '{city}'.");

        if (!_cache.TryGetValue(chatId, GetRequestFormKey(city), out var requestForm))
            throw new NotSupportedException($"Form data was not found for '{city}'.");

        const string OldReplacementString = "ctl00%24MainContent%24txtCode=";
        var newReplacementString = $"{OldReplacementString}{captcha}";

        var requestFormData = requestForm!.Replace(OldReplacementString, newReplacementString);

        var content = new StringContent(requestFormData, Encoding.UTF8, FormDataMediaType);
        var postResponse = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        _htmlDocument.LoadHtml(postResponseResult);

        StringBuilder formBuilder = new();

        foreach (var node in _htmlDocument.SelectNodes("//input"))
        {
            var inputName = node.GetAttributeValue("name");
            var inputValue = node.GetAttributeValue("value");

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

        var formData = formBuilder.ToString();

        content = new StringContent(formData, Encoding.UTF8, FormDataMediaType);
        postResponse = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
        postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        _htmlDocument.LoadHtml(postResponseResult);

        var scheduleTable = _htmlDocument
            .SelectSingleNode("//td[@id='center-panel']")
            .ChildNodes
            .FirstOrDefault(x => x.Name == "table");

        if (scheduleTable is null)
        {
            await _telegramClient.SendMessage(new(chatId, $"Scheduling for '{city.ToUpper()}' was not found."), cToken);
        }
        else
        {
            formBuilder.Clear();

            foreach (var node in _htmlDocument.SelectNodes("//input"))
            {
                var inputName = node.GetAttributeValue("name");
                var inputValue = node.GetAttributeValue("value");

                var encodedInputName = Uri.EscapeDataString(inputName);
                var encodedInputValue = Uri.EscapeDataString(inputValue);

                formBuilder.Append($"&{encodedInputName}={encodedInputValue}");
            }

            formBuilder.Remove(0, 1);

            formData = formBuilder.ToString();

            _cache.AddOrUpdate(chatId, GetResultFormKey(city), formData);

            await _telegramClient.SendMessage(new(chatId, $"Scheduling variants for '{city.ToUpper()}':"), cToken);

            foreach (var item in scheduleTable.SelectNodes("//input[@type='radio']"))
            {
                var appointmentValue = item.GetAttributeValue("value");

                var appointmentText = item.NextSibling.InnerText.Trim();

                _cache.AddOrUpdate(chatId, GetConfirmValueKey(city, appointmentText), appointmentValue);

                await _telegramClient.SendMessage(new(chatId, appointmentText), cToken);
            }

            await _telegramClient.SendMessage(new(chatId, GetConfirmCommand(city)), cToken);
        }
    }
    public async Task Confirm(long chatId, string city, string confirmResult, CancellationToken cToken)
    {
        if (!_cache.TryGetValue(chatId, GetUrlIdentifierKey(city), out var urlIdentifier))
            throw new NotSupportedException($"URL identifier was not found for '{city}'.");

        if (!_cache.TryGetValue(chatId, GetConfirmValueKey(city, confirmResult), out var confirmValue))
            throw new NotSupportedException($"Confirm value was not found for '{city}'.");

        if (!_cache.TryGetValue(chatId, GetResultFormKey(city), out var resultForm))
            throw new NotSupportedException($"Form data was not found for '{city}'.");

        var encodedConfirmValue = Uri.EscapeDataString(confirmValue!);

        const string OldReplacementString = "ctl00%24MainContent%24TextBox1=";
        var newReplacementString = $"{OldReplacementString}{encodedConfirmValue}";

        var stringContent = resultForm!.Replace(OldReplacementString, newReplacementString);

        var content = new StringContent(stringContent, Encoding.UTF8, FormDataMediaType);
        var postResponse = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        if (!string.IsNullOrEmpty(postResponseResult))
            await _telegramClient.SendMessage(new(chatId, postResponseResult), cToken);
        else
            await _telegramClient.SendMessage(new(chatId, $"Something went wrong while confirming for '{city.ToUpper()}'."), cToken);

        _cache.Clear(chatId);
    }
}
