using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;

using Net.Shared.Persistence.Abstractions.Repositories;

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
    private readonly IPersistenceProcessRepository<ITableEntity> _processRepository;

    public TelegramCommandTaskRepository(
        IConfiguration configuration
        , IPersistenceWriterRepository<ITableEntity> writerRepository
        , IPersistenceProcessRepository<ITableEntity> processRepository)
    {
        _writerRepository = writerRepository;
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

    public Task CreateTask(TelegramMessage message, CancellationToken cToken)
    {
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

        return _writerRepository.CreateOne(task, cToken);
    }

    public Task<TelegramCommandTask[]> GetReadyTasks(int limit) =>
        _processRepository.GetProcessableData<TelegramCommandTask>(_hostId, _step, limit);

    public Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks) =>
        _processRepository.SetProcessedData(_hostId, _step, null, tasks);
}
