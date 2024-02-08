using System.Text;

using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Persistence.Repositories;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;
using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;
using KdmidScheduler.Infrastructure.Persistence.MongoDb.Contexts;

using Microsoft.Extensions.Options;

using Net.Shared.Abstractions.Models.Settings;
using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Extensions.Serialization.Json;
using Net.Shared.Persistence.Repositories.MongoDb;

using static KdmidScheduler.Abstractions.Constants;
using static Net.Shared.Persistence.Abstractions.Constants.Enums;

namespace KdmidScheduler.Infrastructure.Persistence.MongoDb.Repositories;

public sealed class KdmidResponseRepository(
    IOptions<CorrelationSettings> correlationOptions,
    IBotCommandsStore commandsStore,
    IKdmidRequestHttpClientCache requestHttpClientCache,
    MongoDbWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> writer,
    MongoDbReaderRepository<KdmidPersistenceContext, KdmidAvailableDates> reader
    ) : IKdmidResponseRepository
{
    private readonly Guid _correlationId = correlationOptions.Value.Id;
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IKdmidRequestHttpClientCache _requestHttpClientCache = requestHttpClientCache;
    private readonly MongoDbWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> _writer = writer;
    private readonly MongoDbReaderRepository<KdmidPersistenceContext, KdmidAvailableDates> _reader = reader;

    public async Task Create(string commandName, string chatId, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var command = await _commandsStore.Create(chatId, commandName, new()
        {
            { BotCommandParametersCityKey, city.ToJson() },
            { BotCommandParametersKdmidIdKey, kdmidId.ToJson() }
        }, cToken);

        await _writer.CreateOne<KdmidAvailableDates>(new()
        {
            Chat = new(chatId),
            City = city,
            Command = command,
            StepId = (int)KdmidProcessSteps.CheckAvailableDates,
            StatusId = (int)ProcessStatuses.Ready,
            CorrelationId = _correlationId

        }, cToken);
    }
    public async Task Update(Command command, string chatId, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        await _commandsStore.Update(chatId, command.Id, command, cToken);

        await _writer.Update<KdmidAvailableDates>(new(x =>
        {
            x.City = city;
            x.Command = command;
            x.StepId = (int)KdmidProcessSteps.CheckAvailableDates;
            x.StatusId = (int)ProcessStatuses.Ready;
        })
        {
            QueryOptions = new(x => x.Chat.Id == chatId && x.Command.Id == command.Id)
        }, cToken);
    }
    public async Task Clear(string chatId, Command command, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var commands = await _commandsStore.Get(chatId, cToken);

        foreach (var _command in commands)
        {
            var _city = _command.Parameters[BotCommandParametersCityKey].FromJson<City>();
            var _kdmidId = _command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

            if (_city.Code == city.Code && _kdmidId.Id == kdmidId.Id)
            {
                await _commandsStore.Delete(chatId, _command.Id, cToken);
            }
        }

        await _writer.Delete<KdmidAvailableDates>(new(x => x.Chat.Id == chatId && x.Command.Id == command.Id), cToken);
        await _requestHttpClientCache.Clear(city, kdmidId, cToken);
    }
    public async Task<string> GetInfo(string chatId, Command command, CancellationToken cToken)
    {
        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();

        var availableDates = await _reader.FindMany<KdmidAvailableDates>(new(x => x.Chat.Id == chatId && x.City.Code == city.Code), cToken);

        var date = DateTime.UtcNow.AddHours(city.TimeShift);

        var result = new StringBuilder();

        result.Append(city.Name);
        result.Append(" - ");
        result.Append(date.ToString("dd.MM.yyyy"));
        result.AppendLine();

        result.AppendLine();

        foreach (var availableDate in availableDates)
        {
            var kdmidId = availableDate.Command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();
            var attempts = availableDate.Command.Parameters[BotCommandParametersAttemptsKey].FromJson<Attempts>();

            result.Append("Id - ");
            result.Append(kdmidId.Id);
            result.AppendLine();

            result.Append("Status - ");
            result.Append(ProcessStatusesMap[availableDate.StatusId]);
            result.AppendLine();

            if (availableDate.StatusId == (int)ProcessStatuses.Error)
            {
                result.Append("Error- ");
                result.Append(availableDate.Error);
                result.AppendLine();
            }

            result.Append("Attempts - ");
            result.Append(attempts.Day == date.Date.DayOfYear ? attempts.Count : 0);
            result.AppendLine();

            result.Append("Last attempt - ");
            result.Append(availableDate.Updated.AddHours(city.TimeShift).ToString("dd.MM.yyyy HH:mm"));
            result.AppendLine();

            result.AppendLine();
        }

        return result.ToString();
    }
}
