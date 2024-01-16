using Azure.Data.Tables;

using KdmidScheduler.Abstractions.Models.Infrastructure.Persistence.AzureTable.v1;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models.Bot;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace KdmidScheduler.Infrastructure.Bots.Stores.AzureTable;

public sealed class KdmidBotCommandsStore(
    IPersistenceReaderRepository<ITableEntity> readerRepository,
    IPersistenceWriterRepository<ITableEntity> writerRepository
    ) : IBotCommandsStore
{
    private readonly IPersistenceReaderRepository<ITableEntity> _readerRepository = readerRepository;
    private readonly IPersistenceWriterRepository<ITableEntity> _writerRepository = writerRepository;

    public async Task<Command> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken)
    {
        var entity = new KdmidBotCommand
        {
            ChatId = chatId,
            Command = new Command(Guid.NewGuid(), Name, Parameters)
        };

        await _writerRepository.CreateOne(entity, cToken);

        return entity.Command;
    }
    public async Task Delete(string chatId, Guid commandId, CancellationToken cToken)
    {
        var deleteOptions = new PersistenceQueryOptions<KdmidBotCommand>
        {
            Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
        };

        await _writerRepository.Delete(deleteOptions, cToken);
    }
    public async Task Update(string chatId, Guid commandId, Command command, CancellationToken cToken)
    {
        var updateOptions = new PersistenceUpdateOptions<KdmidBotCommand>(x => x.Command = command)
        {
            QueryOptions = new PersistenceQueryOptions<KdmidBotCommand>
            {
                Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
            }
        };

        await _writerRepository.Update(updateOptions, cToken);
    }
    public async Task Clear(string chatId, CancellationToken cToken)
    {
        var deleteOptions = new PersistenceQueryOptions<KdmidBotCommand>
        {
            Filter = x => x.ChatId == chatId,
        };

        await _writerRepository.Delete(deleteOptions, cToken);
    }

    public async Task<Command> Get(string chatId, Guid commandId, CancellationToken cToken)
    {
        var queryOptions = new PersistenceQueryOptions<KdmidBotCommand>
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
        var queryOptions = new PersistenceQueryOptions<KdmidBotCommand>
        {
            Filter = x => x.ChatId == chatId,
        };

        var data = await _readerRepository.FindMany(queryOptions, cToken);

        return data.Select(x => x.Command).ToArray();
    }
}
