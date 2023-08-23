using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Exceptions;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Services.CommandProcesses;

public sealed class KdmidCommandProcess : IKdmidCommandProcess
{
    private const string BelgradeId = "blgd";
    private const string BudapestId = "bdpt";
    private const string ParisId = "prs";
    
    public static string CheckBelgradeCommand => $"/{Constants.Kdmid}_{BelgradeId}_chk";
    public static string CheckBudapestCommand => $"/{Constants.Kdmid}_{BudapestId}_chk";
    public static string CheckParisCommand => $"/{Constants.Kdmid}_{ParisId}_chk";

    private static readonly Dictionary<string, KdmidCity> Cities = new()
    {
        { BelgradeId, new(BelgradeId, "belgrad", "Belgrade")},
        { BudapestId, new(BudapestId, "budapest", "Budapest")},
        { ParisId, new(ParisId, "paris", "Paris")}
    };

    private static string GetBaseUrl(KdmidCity city) => $"https://{city.Code}.{Constants.Kdmid}.ru/queue/";
    private static string GetRequestUrl(KdmidCity city, string identifier) => GetBaseUrl(city) + "OrderInfo.aspx?" + identifier;

    private static string GetConfirmDataKey(KdmidCity city) => $"{Constants.Kdmid}.{city.Id}.confirm";
    private static string GetConfirmButtonKey(KdmidCity city, string key) => $"{Constants.Kdmid}.{city.Id}.confirm.button.{key}";

    private readonly TelegramMemoryCache _cache;
    private readonly ITelegramClient _telegramClient;
    private readonly IKdmidHttpClient _httpClient;
    private readonly IKdmidHtmlDocument _htmlDocument;
    private readonly IKdmidCaptchaService _captchaService;

    private readonly Dictionary<string, Func<KdmidCommand, CancellationToken, Task>> _functions;

    public KdmidCommandProcess(
        TelegramMemoryCache cache
        , ITelegramClient telegramClient
        , IKdmidHttpClient httpClient
        , IKdmidHtmlDocument htmlDocument
        , IKdmidCaptchaService captchaService)
    {
        _cache = cache;
        _telegramClient = telegramClient;
        _httpClient = httpClient;
        _htmlDocument = htmlDocument;
        _captchaService = captchaService;

        _functions = new(StringComparer.OrdinalIgnoreCase)
        {
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

        var cityId = message[0..cityIndex].ToString();

        if (!Cities.TryGetValue(cityId, out var city))
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

    public async Task Check(KdmidCommand command, CancellationToken cToken)
    {
        if (!TryGetIdentifier(command.ChatId, command.City, command.Parameters, out var identifier))
        {
            await AskIdentifier(command, cToken);
            return;
        }

        var startPageUrl = GetRequestUrl(command.City, identifier!);

        var startPageString = await _httpClient.GetStartPage(startPageUrl, cToken);

        var startPage = _htmlDocument.GetStartPage(startPageString);

        var captchaUrl = GetBaseUrl(command.City) + startPage.CaptchaCode;

        var captchaImage = await _httpClient.GetCaptchaImage(command.ChatId, captchaUrl, cToken);

        var captchaValue = await _captchaService.SolveInteger(captchaImage, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";

        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaValue}");

        var startPageResultString = await _httpClient.PostStartPageResult(command.ChatId, new(startPageUrl), startPageFormData, cToken);

        var startPageResultFormData = _htmlDocument.GetStartPageResultFormData(startPageResultString);

        await _httpClient.PostConfirmPage(command.ChatId, startPageUrl, startPageResultFormData, cToken);

        var idEndIndex = identifier!.IndexOf('&');
        
        var id = identifier!.Substring(2, idEndIndex);

        var calendarUrl = GetBaseUrl(command.City) + $"SPCalendar.aspx?bjo={id}";

        string calendarPageString = await _httpClient.GetConfirmCalendar(command.ChatId, calendarUrl, cToken);

        var calendarPage = _htmlDocument.GetCalendarPage(calendarPageString);

        if (calendarPage.Variants.Count == 0)
        {
            var text = $"Free spaces in the Russian embassy of {command.City.Name} are not available.";

            var message = new TelegramMessage(command.ChatId, text);

            await _telegramClient.SendMessage(message, cToken);

            return;
        }

        _cache.AddOrUpdate(command.ChatId, GetConfirmDataKey(command.City), calendarPage.FormData);

        var confirmText = $"Free spaces in the Russian embassy of {command.City.Name}.";

        var confirmCommand = $"/{Constants.Kdmid}_{command.City.Id}_cfm?";

        var confirmButtons = calendarPage.Variants
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString("N");

                var buttonKey = GetConfirmButtonKey(command.City, guid);

                _cache.AddOrUpdate(command.ChatId, buttonKey, x.Value);

                return (x.Key, confirmCommand + guid);
            });


        var buttons = new TelegramButtons(command.ChatId, confirmText, confirmButtons);

        await _telegramClient.SendButtons(buttons, cToken);
    }
    public async Task Confirm(KdmidCommand command, CancellationToken cToken)
    {
        if (!TryGetIdentifier(command.ChatId, command.City, null, out var identifier))
        {
            await AskIdentifier(command, cToken);
            return;
        }

        var buttonKey = command.Parameters;

        if (string.IsNullOrEmpty(buttonKey) || !_cache.TryGetValue(command.ChatId, GetConfirmButtonKey(command.City, buttonKey), out var buttonValue))
            throw new ApAzureBotCoreException("Something went wrong. Try again.");

        if (!_cache.TryGetValue(command.ChatId, GetConfirmDataKey(command.City), out var confirmFormData))
            throw new ApAzureBotCoreException("Something went wrong. Try again.");

        var encodedButtonValue = Uri.EscapeDataString(buttonValue!);

        const string ButtonKey = "ctl00%24MainContent%24TextBox1=";

        var data = confirmFormData!.Replace(ButtonKey, $"{ButtonKey}{encodedButtonValue}");

        var url = GetRequestUrl(command.City, identifier!);

        var confirmResult = await _httpClient.GetConfirmPageResult(command.ChatId, url, data, cToken);

        await _telegramClient.SendMessage(new(command.ChatId, confirmResult), cToken);

        _cache.Clear(command.ChatId);
    }

    private Task AskIdentifier(KdmidCommand command, CancellationToken cToken)
    {
        var startCommand = $"/{Constants.Kdmid}_{command.City.Id}_chk?";

        var text = $"Please, send me your valid Russian embassy queue registration identifiers for the {command.City.Name} using the following format:\n\n{startCommand}id=00000&cd=AA000AA0";

        var message = new TelegramMessage(command.ChatId, text);

        return _telegramClient.SendMessage(message, cToken);
    }
    private bool TryGetIdentifier(long chatId, KdmidCity city, string? value, out string? identifier)
    {
        identifier = value;

        var cacheKey = $"{Constants.Kdmid}.{city.Id}.identifier";

        if (identifier is not null)
        {
            var isValidIdentifier =
                identifier.IndexOf("id=") == 0
                && identifier.IndexOf('&') > 0
                && identifier.IndexOf("cd=") > 0;

            if (isValidIdentifier)
                _cache.AddOrUpdate(chatId, cacheKey, identifier);

            return isValidIdentifier;
        }
        else
            return _cache.TryGetValue(chatId, cacheKey, out identifier);
    }
}
