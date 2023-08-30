using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;

using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Models.Contexts;

using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Abstractions.Services;
using Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Exceptions;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;
using Telegram.ApAzureBot.Core.Persistence.Entities;

using static Telegram.ApAzureBot.Core.Constants;

namespace Telegram.ApAzureBot.Core.Services.CommandProcesses;

public sealed class KdmidCommandProcess : IKdmidCommandProcess
{
    public static string BuildCommand(string cityId, string commandId) => $"/{Kdmid.Key}_{cityId}_{commandId}";
    private static string GetConfirmDataKey(KdmidCity city) => $"{Kdmid.Key}.{city.Id}.confirm";
    private static string GetConfirmButtonKey(KdmidCity city, string key) => $"{GetConfirmDataKey(city)}.button.{key}";

    public static readonly Dictionary<string, KdmidCity> Cities = new()
    {
        { Kdmid.Cities.Belgrade, new(Kdmid.Cities.Belgrade, "belgrad", nameof(Kdmid.Cities.Belgrade))},
        { Kdmid.Cities.Budapest, new(Kdmid.Cities.Budapest, "budapest", nameof(Kdmid.Cities.Budapest))},
        { Kdmid.Cities.Paris, new(Kdmid.Cities.Paris, "paris", nameof(Kdmid.Cities.Paris))},
        { Kdmid.Cities.Bucharest, new(Kdmid.Cities.Bucharest, "bucharest", nameof(Kdmid.Cities.Bucharest))},
        { Kdmid.Cities.Riga, new(Kdmid.Cities.Riga, "riga", nameof(Kdmid.Cities.Riga))},
        { Kdmid.Cities.Vilnius, new(Kdmid.Cities.Vilnius, "vilnius", nameof(Kdmid.Cities.Vilnius))},
        { Kdmid.Cities.Sarajevo, new(Kdmid.Cities.Sarajevo, "sarajevo", nameof(Kdmid.Cities.Sarajevo))},
        { Kdmid.Cities.Tirana, new(Kdmid.Cities.Tirana, "tirana", nameof(Kdmid.Cities.Tirana))},
        { Kdmid.Cities.Ljubljana, new(Kdmid.Cities.Ljubljana, "ljubljana", nameof(Kdmid.Cities.Ljubljana))},
        { Kdmid.Cities.Berlin, new(Kdmid.Cities.Berlin, "berlin", nameof(Kdmid.Cities.Berlin))},
        { Kdmid.Cities.Bern, new(Kdmid.Cities.Bern, "bern", nameof(Kdmid.Cities.Bern))},
        { Kdmid.Cities.Brussels, new(Kdmid.Cities.Brussels, "brussels", nameof(Kdmid.Cities.Brussels))},
        { Kdmid.Cities.Dublin, new(Kdmid.Cities.Dublin, "dublin", nameof(Kdmid.Cities.Dublin))},
        { Kdmid.Cities.Helsinki, new(Kdmid.Cities.Helsinki, "helsinki", nameof(Kdmid.Cities.Helsinki))},
        { Kdmid.Cities.Hague, new(Kdmid.Cities.Hague, "hague", nameof(Kdmid.Cities.Hague))},
    };

    private readonly Guid _hostId;
    private readonly TelegramMemoryCache _cache;
    private readonly ITelegramClient _telegramClient;
    private readonly IKdmidHttpClient _httpClient;
    private readonly IKdmidHtmlDocument _htmlDocument;
    private readonly IPersistenceReaderRepository<ITableEntity> _readerRepository;
    private readonly IPersistenceWriterRepository<ITableEntity> _writerRepository;
    private readonly ITelegramCommandTaskRepository _commandTaskRepository;
    private readonly IKdmidCaptchaService _captchaService;

    private readonly Dictionary<string, Func<KdmidCommand, CancellationToken, Task>> _functions;

    public KdmidCommandProcess(
        TelegramMemoryCache cache
        , IConfiguration configuration
        , ITelegramClient telegramClient
        , IKdmidHttpClient httpClient
        , IKdmidHtmlDocument htmlDocument
        , IPersistenceReaderRepository<ITableEntity> readerRepository
        , IPersistenceWriterRepository<ITableEntity> writerRepository
        , ITelegramCommandTaskRepository commandTaskRepository
        , IKdmidCaptchaService captchaService)
    {
        _cache = cache;
        _telegramClient = telegramClient;
        _httpClient = httpClient;
        _htmlDocument = htmlDocument;
        _readerRepository = readerRepository;
        _writerRepository = writerRepository;
        _commandTaskRepository = commandTaskRepository;
        _captchaService = captchaService;

        var hostId = configuration["HostId"];

        ArgumentNullException.ThrowIfNull(hostId, "HostId is not defined");

        if (!Guid.TryParse(hostId, out _hostId))
            throw new ArgumentException("HostId is not valid");

        _functions = new(StringComparer.OrdinalIgnoreCase)
        {
            { Kdmid.Commands.Menu, Menu },
            { Kdmid.Commands.Request, Request },
            { Kdmid.Commands.Seek, Seek },
            { Kdmid.Commands.Confirm, Confirm },
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

        var commandId = commandParametersIndex > 0
            ? message[0..commandParametersIndex].ToString()
            : message.ToString();

        var commandParameters = commandParametersIndex > 0
            ? message[(commandParametersIndex + 1)..]
            : ReadOnlySpan<char>.Empty;

        var command = new KdmidCommand(chatId, commandId, city, commandParameters.IsEmpty ? null : commandParameters.ToString());

        return !_functions.TryGetValue(commandId.ToString(), out var call)
            ? throw new ApAzureBotCoreException($"The command: {commandId} is not supported.")
            : call(command, cToken);
    }

    public async Task Menu(KdmidCommand command, CancellationToken cToken)
    {
        var message = new TelegramMessage(command.ChatId, BuildCommand(command.City.Id, Kdmid.Commands.Request));

        var isScheduled = await _commandTaskRepository.IsExists(message, cToken);

        var buttons = new List<(string, string)>
        {
            ("Request", BuildCommand(command.City.Id, Kdmid.Commands.Request)),
        };

        if (!isScheduled)
            buttons.Add(("Start seek", BuildCommand(command.City.Id, Kdmid.Commands.Seek) + "?start"));
        else
            buttons.Add(("Stop seek", BuildCommand(command.City.Id, Kdmid.Commands.Seek) + "?stop"));

        var menuButton = new TelegramButtons(command.ChatId, $"Choose the action for {command.City.Name}.", buttons, ButtonStyle.Horizontally);

        await _telegramClient.SendButtons(menuButton, cToken);
    }
    public async Task Seek(KdmidCommand command, CancellationToken cToken)
    {
        if (string.IsNullOrEmpty(command.Parameters))
            throw new ApAzureBotCoreException("The command parameters for the seek is empty.");

        string text;
        if (command.Parameters.Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            var message = new TelegramMessage(command.ChatId, BuildCommand(command.City.Id, Kdmid.Commands.Request));
            await _commandTaskRepository.StartTask(message, cToken);
            text = $"Auto seek for {command.City.Name} was started.";
        }
        else if (command.Parameters.Equals("stop", StringComparison.OrdinalIgnoreCase))
        {
            var message = new TelegramMessage(command.ChatId, BuildCommand(command.City.Id, Kdmid.Commands.Request));
            await _commandTaskRepository.StopTask(message, cToken);
            text = $"Auto seek for {command.City.Name} was stopped.";
        }
        else
            throw new ApAzureBotCoreException($"The command parameters: {command.Parameters} is not supported.");

        await _telegramClient.SendMessage(new(command.ChatId, text), cToken);
    }

    public async Task Request(KdmidCommand command, CancellationToken cToken)
    {
        var identifier = await GetOrAddIdentifier(command.ChatId, command.City.Id, command.Parameters, cToken);

        if (identifier is null)
        {
            await AskIdentifier(command, cToken);
            return;
        }

        var startPageResponse = await _httpClient.GetStartPage(command.City, identifier, cToken);

        var startPage = _htmlDocument.GetStartPage(startPageResponse);

        var captchaImage = await _httpClient.GetCaptchaImage(command.ChatId, command.City, startPage.CaptchaCode, cToken);

        var captchaResult = await _captchaService.SolveInteger(captchaImage, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";

        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaResult}");

        var startPageResultResponse = await _httpClient.PostStartPageResult(command.ChatId, command.City, identifier, startPageFormData, cToken);

        var startPageResultFormData = _htmlDocument.GetStartPageResultFormData(startPageResultResponse);

        var confirmPageResultResponse = await _httpClient.PostConfirmPageResult(command.City, identifier, startPageResultFormData, cToken);

        var confirmPage = _htmlDocument.GetConfirmPage(confirmPageResultResponse);

        if (confirmPage.Variants.Count == 0)
        {
            //var text = $"Accessible spaces for scheduling at the Russian embassy in {command.City.Name} are not available.";

            //var message = new TelegramMessage(command.ChatId, text);

            //await _telegramClient.SendMessage(message, cToken);

            return;
        }

        _cache.AddOrUpdate(command.ChatId, GetConfirmDataKey(command.City), confirmPage.FormData);

        var confirmText = $"Accessible spaces for scheduling at the Russian embassy in {command.City.Name}.";

        var confirmCommand = BuildCommand(command.City.Id, Kdmid.Commands.Confirm);

        var confirmButtons = confirmPage.Variants
            .Select(x =>
            {
                var guid = Guid.NewGuid().ToString("N");

                var buttonKey = GetConfirmButtonKey(command.City, guid);

                _cache.AddOrUpdate(command.ChatId, buttonKey, x.Value);

                return (x.Key, $"{confirmCommand}?{guid}");
            });

        var buttons = new TelegramButtons(command.ChatId, confirmText, confirmButtons, ButtonStyle.Vertically);

        await _telegramClient.SendButtons(buttons, cToken);
    }
    public async Task Confirm(KdmidCommand command, CancellationToken cToken)
    {
        var identifier = await GetOrAddIdentifier(command.ChatId, command.City.Id, null, cToken);

        if (identifier is null)
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

        var confirmResult = await _httpClient.PostConfirmPageResult(command.ChatId, command.City, identifier, data, cToken);

        await _telegramClient.SendMessage(new(command.ChatId, confirmResult), cToken);

        _cache.Clear(command.ChatId);
    }

    private Task AskIdentifier(KdmidCommand command, CancellationToken cToken)
    {
        var templateCommand = BuildCommand(command.City.Id, command.Id);

        var text = $"Please, send me your valid Russian embassy queue registration identifiers for the {command.City.Name} using the following format:\n\n{templateCommand}?id=00000&cd=AA000AA0";

        var message = new TelegramMessage(command.ChatId, text);

        return _telegramClient.SendMessage(message, cToken);
    }
    private async Task<string?> GetOrAddIdentifier(long chatId, string cityId, string? value, CancellationToken cToken)
    {
        var identifier = value;

        var cacheKey = $"{Kdmid.Key}.{cityId}.identifier";

        var cacheQueryOptions = new PersistenceQueryOptions<TelegramCommandCache>
        {
            Filter = x => x.PartitionKey == _hostId.ToString() && x.ChatId == chatId && x.Key == cacheKey
        };

        if (identifier is not null)
        {
            var isValidIdentifier =
                identifier.IndexOf("id=") == 0
                && identifier.IndexOf('&') > 0
                && identifier.IndexOf("cd=") > 0;

            if (isValidIdentifier)
            {
                _cache.AddOrUpdate(chatId, cacheKey, identifier);

                if (!await _readerRepository.IsExists(cacheQueryOptions, cToken))
                {
                    await _writerRepository.CreateOne(new TelegramCommandCache
                    {
                        PartitionKey = _hostId.ToString(),
                        RowKey = Guid.NewGuid().ToString(),

                        ChatId = chatId,
                        Key = cacheKey,
                        Value = identifier,

                    }, cToken);
                }
            }

            return identifier;
        }
        else
        {
            if (_cache.TryGetValue(chatId, cacheKey, out identifier))
                return identifier;

            var cache = await _readerRepository.FindSingle(cacheQueryOptions, cToken);

            return cache?.Value;
        }
    }
}
