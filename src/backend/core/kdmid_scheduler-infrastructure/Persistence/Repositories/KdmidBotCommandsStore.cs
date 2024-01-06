﻿using KdmidScheduler.Abstractions.Models.v1.Persistence.MongoDb;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Persistence.Abstractions.Models.Contexts;

namespace KdmidScheduler.Infrastructure.Persistence.Repositories;

public sealed class KdmidBotCommandsStore(IPersistenceNoSqlReaderRepository readerRepository, IPersistenceNoSqlWriterRepository writerRepository) : IBotCommandsStore
{
    private readonly IPersistenceNoSqlReaderRepository _readerRepository = readerRepository;
    private readonly IPersistenceNoSqlWriterRepository _writerRepository = writerRepository;

    public async Task<BotCommand> Create(string chatId, string Name, Dictionary<string, string> Parameters, CancellationToken cToken)
    {
        var entity = new KdmidBotCommands
        {
            ChatId = chatId,
            Command = new BotCommand(Guid.NewGuid(), Name, Parameters)
        };

        await _writerRepository.CreateOne(entity, cToken);

        return entity.Command;
    }
    public async Task Delete(string chatId, Guid commandId, CancellationToken cToken)
    {
        var deleteOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId && x.Command.Id == commandId,
        };

        await _writerRepository.Delete(deleteOptions, cToken);
    }
    public async Task Update(string chatId, Guid commandId, BotCommand command, CancellationToken cToken)
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

    public async Task<BotCommand> Get(string chatId, Guid commandId, CancellationToken cToken)
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
    public async Task<BotCommand[]> Get(string chatId, CancellationToken cToken)
    {
        var queryOptions = new PersistenceQueryOptions<KdmidBotCommands>
        {
            Filter = x => x.ChatId == chatId,
        };

        var data = await _readerRepository.FindMany(queryOptions, cToken);

        return data.Select(x => x.Command).ToArray();
    }
}