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
    
    public static string CheckBelgradeCommand => GetCheckCommand(BelgradeId);
    public static string CheckBudapestCommand => GetCheckCommand(BudapestId);
    public static string CheckParisCommand => GetCheckCommand(ParisId);

    private static readonly Dictionary<string, KdmidCity> Cities = new()
    {
        { BelgradeId, new(BelgradeId, "belgrad", "Belgrade")},
        { BudapestId, new(BudapestId, "budapest", "Budapest")},
        { ParisId, new(ParisId, "paris", "Paris")}
    };

    private static string GetCheckCommand(string cityId) => $"/{Constants.Kdmid}_{cityId}_chk";
    private static string GetConfirmCommand(string cityId) => $"/{Constants.Kdmid}_{cityId}_cfm";
    
    private static string GetConfirmDataKey(KdmidCity city) => $"{Constants.Kdmid}.{city.Id}.confirm";
    private static string GetConfirmButtonKey(KdmidCity city, string key) => $"{GetConfirmDataKey(city)}.button.{key}";

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
            throw new ApAzureBotCoreException("The message is empty.");

        var cityIndex = message.IndexOf('_');

        if (cityIndex < 0)
            throw new ApAzureBotCoreException("The city index is not found.");

        var cityId = message[0..cityIndex].ToString();

        if (!Cities.TryGetValue(cityId, out var city))
            throw new ApAzureBotCoreException($"The cityId: {cityId} is not supported.");

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
            ? throw new ApAzureBotCoreException($"The command: {commandCode} is not supported.")
            : call(command, cToken);
    }

    public async Task Check(KdmidCommand command, CancellationToken cToken)
    {
        if (!TryGetIdentifier(command.ChatId, command.City, command.Parameters, out var identifier))
        {
            await AskIdentifier(command, cToken);
            return;
        }

        var startPageResponse = await _httpClient.GetStartPage(command.City, identifier!, cToken);

        var startPage = _htmlDocument.GetStartPage(startPageResponse);

        var captchaImage = await _httpClient.GetCaptchaImage(command.ChatId, command.City, startPage.CaptchaCode , cToken);

        var captchaResult = await _captchaService.SolveInteger(captchaImage, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";

        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaResult}");

        var startPageResultResponse = await _httpClient.PostStartPageResult(command.ChatId, command.City, identifier!, startPageFormData, cToken);

        var startPageResultFormData = _htmlDocument.GetStartPageResultFormData(startPageResultResponse);

        var confirmPageResultResponse = await _httpClient.PostConfirmPageResult(command.City, identifier!, startPageResultFormData, cToken);

        var confirmPage = _htmlDocument.GetConfirmPage(confirmPageResultResponse);

        if (confirmPage.Variants.Count == 0)
        {
            var text = $"Accessible spaces for scheduling at the Russian embassy in {command.City.Name} are not available.";

            var message = new TelegramMessage(command.ChatId, text);

            await _telegramClient.SendMessage(message, cToken);

            return;
        }

        _cache.AddOrUpdate(command.ChatId, GetConfirmDataKey(command.City), confirmPage.FormData);

        var confirmText = $"Accessible spaces for scheduling at the Russian embassy in {command.City.Name}.";

        var confirmCommand = GetConfirmCommand(command.City.Id);

        var confirmButtons = confirmPage.Variants
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString("N");

                var buttonKey = GetConfirmButtonKey(command.City, guid);

                _cache.AddOrUpdate(command.ChatId, buttonKey, x.Value);

                return (x.Key, $"{confirmCommand}?{guid}");
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
            throw new ApAzureBotCoreException("The button key is not found in the cache.");

        if (!_cache.TryGetValue(command.ChatId, GetConfirmDataKey(command.City), out var confirmFormData))
            throw new ApAzureBotCoreException("The confirm form data is not found in the cache.");

        var encodedButtonValue = Uri.EscapeDataString(buttonValue!);

        const string ButtonKey = "ctl00%24MainContent%24TextBox1=";

        var data = confirmFormData!.Replace(ButtonKey, $"{ButtonKey}{encodedButtonValue}");

        var confirmResult = await _httpClient.PostConfirmPageResult(command.ChatId, command.City, identifier!, data, cToken);

        await _telegramClient.SendMessage(new(command.ChatId, confirmResult), cToken);

        _cache.Clear(command.ChatId);
    }

    private Task AskIdentifier(KdmidCommand command, CancellationToken cToken)
    {
        var templateCommand = GetCheckCommand(command.City.Id);

        var text = $"Please, send me your valid Russian embassy queue registration identifiers for the {command.City.Name} using the following format:\n\n{templateCommand}?id=00000&cd=AA000AA0";

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
