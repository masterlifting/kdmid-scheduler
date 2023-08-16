using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Repositories;

using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Models;
using Telegram.ApAzureBot.Core.Persistence.Entities;

namespace Telegram.ApAzureBot.Infrastructure.Persistence.Repositories;

public sealed class TelegramCommandTaskRepository : ITelegramCommandTaskRepository
{
    private readonly Guid _hostId;
    private readonly TelegramCommandStep _step;

    private readonly Lazy<IPersistenceWriterRepository<ITableEntity>> _writerRepository;
    private readonly Lazy<IPersistenceProcessRepository<ITableEntity>> _processRepository;

    public TelegramCommandTaskRepository(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var hostId = configuration["HostId"];

        ArgumentNullException.ThrowIfNull(hostId, "HostId is not defined");

        if (!Guid.TryParse(hostId, out _hostId))
        {
            throw new ArgumentException("HostId is not valid");
        }

        _step = new()
        {
            Id = (int)Core.Constants.TelegramCommandTaskStep.Process,
            Name = Core.Constants.TelegramCommandTaskStep.Process.ToString(),
            Created = DateTime.UtcNow,
            Description = "Process telegram command",
            DocumentVersion = "1.0"
        };

        _writerRepository = new(serviceProvider.GetRequiredService<IPersistenceWriterRepository<ITableEntity>>);
        _processRepository = new(serviceProvider.GetRequiredService<IPersistenceProcessRepository<ITableEntity>>);
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
            StatusId = (int)Core.Constants.TelegramCommandTaskStatus.Created,
        };
        return _writerRepository.Value.CreateOne(task, cToken);
    }

    public Task<TelegramCommandTask[]> GetReadyTasks(int limit) =>
        _processRepository.Value.GetProcessableData<TelegramCommandTask>(_hostId, _step, limit);

    public Task UpdateTaskStatus(IEnumerable<TelegramCommandTask> tasks) =>
        _processRepository.Value.SetProcessedData(_hostId, _step, null, tasks);
}
