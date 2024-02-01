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
    MongoDbWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> writer
    ) : IKdmidResponseRepository
{
    private readonly Guid _correlationId = correlationOptions.Value.Id;
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IKdmidRequestHttpClientCache _requestHttpClientCache = requestHttpClientCache;
    private readonly MongoDbWriterRepository<KdmidPersistenceContext, KdmidAvailableDates> _writer = writer;

    public async Task Create(string commandName, string chatId, City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var command = await _commandsStore.Create(chatId, commandName, new()
        {
            { BotCommandParametersCityKey, city.ToJson() },
            { BotCommandParametersKdmidIdKey, kdmidId.ToJson() }
        }, cToken);

        await _writer.CreateOne<KdmidAvailableDates>(new()
        {
            Chat = new(chatId, new(string.Empty)),
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
    public async Task Clear(string chatId, Guid commandId, CancellationToken cToken)
    {
        var command = await _commandsStore.Get(chatId, commandId, cToken);
        
        if (command is null)
            return;
        
        await _commandsStore.Delete(chatId, commandId, cToken);
        await _writer.Delete<KdmidAvailableDates>(new(x => x.Chat.Id == chatId && x.Command.Id == commandId), cToken);

        var city = command.Parameters[BotCommandParametersCityKey].FromJson<City>();
        var kdmidId = command.Parameters[BotCommandParametersKdmidIdKey].FromJson<KdmidId>();

        await _requestHttpClientCache.Clear(city, kdmidId, cToken);
    }
}
