using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.MongoDb.v1;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace KdmidScheduler.Infrastructure.Bots.Stores.MongoDb;

public sealed class KdmidBotCommandsStore(
    IPersistenceReaderRepository<IPersistentNoSql> readerRepository,
    IPersistenceWriterRepository<IPersistentNoSql> writerRepository) : IBotCommandsStore
{
    private readonly IPersistenceReaderRepository<IPersistentNoSql> _readerRepository = readerRepository;
    private readonly IPersistenceWriterRepository<IPersistentNoSql> _writerRepository = writerRepository;

    public Task Create(string chatId, Command command, CancellationToken cToken)
    {
        var entity = new KdmidBotCommands
        {
            ChatId = chatId,
            Command = command
        };

        return _writerRepository.CreateOne(entity, cToken);
    }
    public async Task<Command> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken)
    {
        var command = new Command(Guid.NewGuid(), Name, Parameters);

        await Create(chatId, command, cToken);

        return command;
    }
    public async Task Delete(string chatId, Guid commandId, CancellationToken cToken)
    {
        var deleteOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
        };

        await _writerRepository.Delete(deleteOptions, cToken);
    }
    public async Task Update(string chatId, Guid commandId, Command command, CancellationToken cToken)
    {
        var updateOptions = new PersistenceUpdateOptions<KdmidBotCommands>(x => x.Command = command)
        {
            QueryOptions = new PersistenceQueryOptions<KdmidBotCommands>
            {
                Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
            }
        };

        await _writerRepository.Update(updateOptions, cToken);
    }
    public async Task Clear(string chatId, CancellationToken cToken)
    {
        var deleteOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId,
        };

        await _writerRepository.Delete(deleteOptions, cToken);
    }

    public async Task<Command> Get(string chatId, Guid commandId, CancellationToken cToken)
    {
        var queryOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
        };

        var queryResult = await _readerRepository.FindSingle(queryOptions, cToken);

        return queryResult is not null
            ? queryResult.Command
            : throw new InvalidOperationException($"The command '{commandId}' was not found");
    }
    public async Task<Command[]> Get(string chatId, CancellationToken cToken)
    {
        var queryOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId,
        };

        var data = await _readerRepository.FindMany(queryOptions, cToken);

        return data.Select(x => x.Command).ToArray();
    }
}
