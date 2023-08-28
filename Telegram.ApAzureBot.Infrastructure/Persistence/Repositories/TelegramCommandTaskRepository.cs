using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;

using Net.Shared.Persistence.Abstractions.Repositories;
using Net.Shared.Persistence.Models.Contexts;

using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Persistence.Entities;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Telegram.ApAzureBot.Infrastructure.Persistence.Repositories;

public sealed class TelegramCommandTaskRepository : ITelegramCommandTaskRepository
{
    private readonly Guid _hostId;
    private readonly TelegramCommandStep _step;

    private readonly IPersistenceWriterRepository<ITableEntity> _writerRepository;
    private readonly IPersistenceReaderRepository<ITableEntity> _readerRepository;
    private readonly IPersistenceProcessRepository<ITableEntity> _processRepository;

    public TelegramCommandTaskRepository(
        IConfiguration configuration
        , IPersistenceWriterRepository<ITableEntity> writerRepository
        , IPersistenceReaderRepository<ITableEntity> readerRepository
        , IPersistenceProcessRepository<ITableEntity> processRepository)
    {
        _writerRepository = writerRepository;
        _readerRepository = readerRepository;
        _processRepository = processRepository;

        var hostId = configuration["HostId"];

        ArgumentNullException.ThrowIfNull(hostId, "HostId is not defined");

        if (!Guid.TryParse(hostId, out _hostId))
            throw new ArgumentException("HostId is not valid");

        _step = new()
        {
            Id = (int)Core.Constants.ProcessSteps.Process,
            Name = Core.Constants.ProcessSteps.Process.ToString(),
            Created = DateTime.UtcNow,
            Description = "Process telegram command",
            DocumentVersion = "1.0"
        };
    }

    public Task<bool> IsExists(TelegramMessage message, CancellationToken cToken)
    {
        Func< TelegramCommandTask, bool> statusPredicate = x => 
            !(x.StatusId == (int)ProcessStatuses.Completed || x.StatusId == (int)ProcessStatuses.Draft);
        
        return IsExists(message, statusPredicate, cToken);
    }

    public async Task StartTask(TelegramMessage message, CancellationToken cToken)
    {
        Func<TelegramCommandTask, bool> statusPredicate = x => true;

        if (await IsExists(message, statusPredicate, cToken))
        {
            await UpdateTask(message, ProcessStatuses.Ready, cToken);
            return;
        }

        var task = new TelegramCommandTask()
        {
            PartitionKey = _hostId.ToString(),
            RowKey = Guid.NewGuid().ToString(),

            ChatId = message.ChatId,
            Text = message.Text,

            HostId = _hostId,
            StepId = _step.Id,
            StatusId = (int)ProcessStatuses.Ready,
        };

        await _writerRepository.CreateOne(task, cToken);
    }

    public Task StopTask(TelegramMessage message, CancellationToken cToken) =>
        UpdateTask(message, ProcessStatuses.Draft, cToken);

    public async Task<TelegramCommandTask[]> GetReadyTasks(int limit)
    {
        var unprocessedData = await _processRepository.GetUnprocessedData<TelegramCommandTask>(_hostId, _step, limit, DateTime.UtcNow, 20);
        var processableData = await _processRepository.GetProcessableData<TelegramCommandTask>(_hostId, _step, limit);

        return processableData.Concat(unprocessedData).ToArray();
    }

    public Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks) =>
        _processRepository.SetProcessedData(_hostId, _step, null, tasks);

    private Task UpdateTask(TelegramMessage message, ProcessStatuses status, CancellationToken cToken)
    {
        var updated = DateTime.UtcNow;

        var updater = (TelegramCommandTask x) =>
        {
            x.StatusId = (int)status;
            x.Attempt = 0;
            x.Description = null;
            x.Error = null;
            x.Updated = updated;
        };

        var updateOptions = new PersistenceUpdateOptions<TelegramCommandTask>(updater, nameof(TelegramCommandTask.RowKey))
        {
            QueryOptions = new()
            {
                Filter = x =>
                    x.PartitionKey == _hostId.ToString()
                    && x.ChatId == message.ChatId
                    && x.Text == message.Text
                    && x.StepId == _step.Id
            }
        };

        return _writerRepository.Update(updateOptions, cToken);
    }

    private Task<bool> IsExists(TelegramMessage message, Func<TelegramCommandTask, bool> statusPredicate, CancellationToken cToken)
    {
        var queryOptions = new PersistenceQueryOptions<TelegramCommandTask>
        {
            Filter = x =>
                x.PartitionKey == _hostId.ToString()
                && x.ChatId == message.ChatId
                && x.Text == message.Text
                && x.StepId == _step.Id
                && statusPredicate(x)
        };

        return _readerRepository.IsExists(queryOptions, cToken);
    }
}
