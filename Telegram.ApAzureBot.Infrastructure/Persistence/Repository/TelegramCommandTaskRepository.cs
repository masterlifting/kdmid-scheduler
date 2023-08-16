using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Repositories.NoSql;

using Telegram.ApAzureBot.Core.Persistence;
using Telegram.ApAzureBot.Core.Persistence.NoSql;

namespace Telegram.ApAzureBot.Infrastructure.Persistence.Repository;

public sealed class TelegramCommandTaskRepository : ITelegramCommandTaskRepository
{
    private readonly Guid _hostId;
    private readonly TelegramCommandStep _step;

    private readonly Lazy<IPersistenceNoSqlWriterRepository> _writerRepository;
    private readonly Lazy<IPersistenceNoSqlProcessRepository> _processRepository;

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

        _writerRepository = new(serviceProvider.GetRequiredService<IPersistenceNoSqlWriterRepository>);
        _processRepository = new(serviceProvider.GetRequiredService<IPersistenceNoSqlProcessRepository>);
    }

    public Task CreateCommandTask(TelegramCommandTask task, CancellationToken cToken) => 
        _writerRepository.Value.CreateOne(task, cToken);

    public Task<TelegramCommandTask[]> GetReadyTasks(int limit) => 
        _processRepository.Value.GetProcessableData<TelegramCommandTask>(_hostId, _step, limit);

    public Task UpdateStatus(IEnumerable<TelegramCommandTask> tasks) => 
        _processRepository.Value.SetProcessedData(_hostId, _step, null, tasks);
}
