using System.Text;

using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Html;
using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Http;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Models.CommandProcesses;

namespace Telegram.ApAzureBot.Core.Services.CommandProcesses;

public sealed class KdmidCommandProcess : IKdmidCommandProcess
{
    private static readonly Dictionary<string, string> KdmidCities = new()
    {
        {"blgd","belgrad" },
        {"bdpt","budapest" },
    };
    private static readonly Dictionary<string, string> ReadableCities = new()
    {
        {"belgrad","Belgrade" },
        {"budapest","Budapest" },
    };
    private const string FormDataMediaType = "application/x-www-form-urlencoded";
    private static string GetScheduleCommand(string city) => $"/{Constants.Kdmid}_{city}_sch?";
    private static string GetCheckCommand(string city) => $"/{Constants.Kdmid}_{city}_chk?";
    private static string GetConfirmCommand(string city) => $"/{Constants.Kdmid}_{city}_cfm?";
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

    private readonly Dictionary<string, Func<KdmidCommand, CancellationToken, Task>> _functions;

    public KdmidCommandProcess(
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
            { "sch", Schedule },
            { "chk", Check },
            { "cfm", Confirm },
        };
    }

    public Task Start(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
    {
        if (message.Length == 0)
            throw new NotSupportedException("The command is not supported.");

        var cityIndex = message.IndexOf('_');

        if (cityIndex < 0)
            throw new NotSupportedException("The command is not supported.");

        var cityCode = message[0..cityIndex].ToString();

        if (!KdmidCities.TryGetValue(cityCode, out var city))
            throw new NotSupportedException("The command is not supported.");

        message = message[(cityIndex + 1)..];

        var commandParametersIndex = message.IndexOf('?');

        var commandCode = commandParametersIndex > 0
            ? message[0..commandParametersIndex]
            : message;

        var commandParameters = commandParametersIndex > 0
            ? message[(commandParametersIndex + 1)..]
            : ReadOnlySpan<char>.Empty;

        var command = new KdmidCommand(chatId, city, commandParameters.IsEmpty ? null : commandParameters.ToString());

        return !_functions.TryGetValue(commandCode.ToString(), out var call)
            ? throw new NotSupportedException("The command is not supported.")
            : call(command, cToken);
    }

    public async Task Schedule(KdmidCommand command, CancellationToken cToken)
    {
        var urlIdentifier = command.Parameters;

        if (urlIdentifier is not null)
        {
            var isValid =
                urlIdentifier.IndexOf("id=") == 0
                && urlIdentifier.IndexOf('&') > 0
                && urlIdentifier.IndexOf("cd=") > 0;

            if (!isValid)
            {
                await AskIdentifiers(command, cToken);
                return;
            }

            _cache.AddOrUpdate(command.ChatId, GetUrlIdentifierKey(command.City), urlIdentifier);
        }

        if (!_cache.TryGetValue(command.ChatId, GetUrlIdentifierKey(command.City), out urlIdentifier))
        {
            await AskIdentifiers(command, cToken);
            return;
        }

        /*/
        var page = await _httpClient.GetStringAsync(GetRequestUrl(command.City, urlIdentifier!), cToken);
        /*/
        var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/firstResponse.html");
        //*/

        _htmlDocument.LoadHtml(page);

        var pageNodes = _htmlDocument.SelectNodes("//input | //img");

        if (pageNodes is null || !pageNodes.Any())
            throw new NotSupportedException($"Page data for {ReadableCities[command.City]} was not found.");

        string? captchaUrl = null;

        StringBuilder formBuilder = new();

        foreach (var node in pageNodes)
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
                    captchaUrl = GetBaseUrl(command.City) + captchaUrlPart;
                }
            }
        }

        formBuilder.Remove(0, 1);

        var formData = formBuilder.ToString();

        _cache.AddOrUpdate(command.ChatId, GetRequestFormKey(command.City), formData);

        if (captchaUrl is null)
            throw new ArgumentException("Captcha is not found.");
        else
        {
            /*/
            var captcha = await _httpClient.GetByteArrayAsync(captchaUrl, cToken);
            var captchaResult = await _captchaService.SolveInteger(captcha, cToken);
            await Check(new(command.ChatId, command.City, captchaResult.ToString()), cToken);
            /*/
            var captcha = File.ReadAllBytes(Environment.CurrentDirectory + "/Content/CodeImage.jpeg");
            var captchaResult = await _captchaService.SolveInteger(captcha, cToken);
            await Check(new(command.ChatId, command.City, captchaResult.ToString()), cToken);
            //await _telegramClient.SendPhoto(new(chatId, captcha, "captcha.jpeg", GetCaptchaCommand(city)), cToken);
            //*/
        }
    }
    public async Task Check(KdmidCommand command, CancellationToken cToken)
    {
        if (command.Parameters is not null && command.Parameters.Length < 6 || !uint.TryParse(command.Parameters, out _))
            throw new NotSupportedException("Something went wrong. Try again.");

        if (!_cache.TryGetValue(command.ChatId, GetUrlIdentifierKey(command.City), out var urlIdentifier))
        {
            await AskIdentifiers(command, cToken);
            return;
        }

        if (!_cache.TryGetValue(command.ChatId, GetRequestFormKey(command.City), out var requestForm))
            throw new NotSupportedException("Something went wrong. Try again.");

        const string OldReplacementString = "ctl00%24MainContent%24txtCode=";
        var newReplacementString = $"{OldReplacementString}{command.Parameters}";

        var requestFormData = requestForm!.Replace(OldReplacementString, newReplacementString);

        /*/
        var content = new StringContent(requestFormData, Encoding.UTF8, FormDataMediaType);
        var postResponse = await _httpClient.PostAsync(GetRequestUrl(command.City, urlIdentifier!), content, cToken);
        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);
        /*/
        var postResponseResult = File.ReadAllText(Environment.CurrentDirectory + "/Content/secondResponse.html");
        //*/

        _htmlDocument.LoadHtml(postResponseResult);

        var pageNodes = _htmlDocument.SelectNodes("//input");

        if (pageNodes is null || !pageNodes.Any())
            throw new NotSupportedException($"Page data for {ReadableCities[command.City]} was not found.");

        StringBuilder formBuilder = new();

        foreach (var node in pageNodes)
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

        /*/
        content = new StringContent(formData, Encoding.UTF8, FormDataMediaType);
        postResponse = await _httpClient.PostAsync(GetRequestUrl(command.City, urlIdentifier!), content, cToken);
        postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);
        /*/
        postResponseResult = File.ReadAllText(Environment.CurrentDirectory + "/Content/thirdResponse_Ok.html");
        //*/

        _htmlDocument.LoadHtml(postResponseResult);

        var resultTable = _htmlDocument
            .SelectSingleNode("//td[@id='center-panel']")
            .ChildNodes
            .FirstOrDefault(x => x.Name == "table");

        if (resultTable is null)
        {
            var text = $"Free spaces in the Russian embassy of {ReadableCities[command.City]} are not available.";
            var message = new TelegramMessage(command.ChatId, text);
            await _telegramClient.SendMessage(message, cToken);
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

            _cache.AddOrUpdate(command.ChatId, GetResultFormKey(command.City), formData);

            var confirmationText = $"Free spaces in the Russian embassy of {ReadableCities[command.City]}.";

            var confirmationValues = new List<(string, string)>(22);

            var cityCode = KdmidCities.FirstOrDefault(x => x.Value == command.City).Key;

            foreach (var item in resultTable.SelectNodes("//input[@type='radio']"))
            {
                var appointmentValue = item.GetAttributeValue("value");

                var buttonName = item.NextSibling.InnerText.Trim();

                var buttonValue = $"{GetConfirmCommand(command.City)}{appointmentValue}";

                var guid = Guid.NewGuid().ToString("N");

                var confirmKey = GetConfirmValueKey(command.City, guid);

                _cache.AddOrUpdate(command.ChatId, confirmKey, appointmentValue);

                confirmationValues.Add((buttonName, GetConfirmCommand(cityCode) + guid));
            }

            var confirmButtons = new TelegramButtons(command.ChatId, confirmationText, confirmationValues);

            await _telegramClient.SendButtons(confirmButtons, cToken);
        }
    }
    public async Task Confirm(KdmidCommand command, CancellationToken cToken)
    {
        if (command.Parameters is null)
            throw new NotSupportedException("Confirm value is not valid.");

        if (!_cache.TryGetValue(command.ChatId, GetUrlIdentifierKey(command.City), out var urlIdentifier))
            throw new NotSupportedException("URL identifier is not found.");

        if (!_cache.TryGetValue(command.ChatId, GetConfirmValueKey(command.City, command.Parameters), out var confirmValue))
            throw new NotSupportedException("Confirm value is not found.");

        if (!_cache.TryGetValue(command.ChatId, GetResultFormKey(command.City), out var resultForm))
            throw new NotSupportedException("Result data are not found.");

        var encodedConfirmValue = Uri.EscapeDataString(confirmValue!);

        const string OldReplacementString = "ctl00%24MainContent%24TextBox1=";
        var newReplacementString = $"{OldReplacementString}{encodedConfirmValue}";

        var stringContent = resultForm!.Replace(OldReplacementString, newReplacementString);

        /*/
        var content = new StringContent(stringContent, Encoding.UTF8, FormDataMediaType);
        var postResponse = await _httpClient.PostAsync(GetRequestUrl(command.City, urlIdentifier!), content, cToken);
        var postResponseResult = await postResponse.Content.ReadAsStringAsync(cToken);

        if (!string.IsNullOrEmpty(postResponseResult))
            await _telegramClient.SendMessage(new(command.ChatId, postResponseResult), cToken);
        else
            await _telegramClient.SendMessage(new(command.ChatId, $"Something went wrong while {ReadableCities[command.City]} confirming ."), cToken);
        /*/
        await _telegramClient.SendMessage(new(command.ChatId, "Confirmed."), cToken);
        //*/

        _cache.Clear(command.ChatId);
    }

    private Task AskIdentifiers(KdmidCommand command, CancellationToken cToken)
    {
        var cityCode = KdmidCities.FirstOrDefault(x => x.Value == command.City).Key;
        var scheduleCommand = GetScheduleCommand(cityCode);
        var responseText = $"Please, send me your Russian embassy queue registration identifiers for {ReadableCities[command.City]} using the following format:\n\n{scheduleCommand}id=00000&cd=AA000AA0";
        var responseMessage = new TelegramMessage(command.ChatId, responseText);

        return _telegramClient.SendMessage(responseMessage, cToken);
    }
}
