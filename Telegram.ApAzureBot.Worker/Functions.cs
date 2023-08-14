using Microsoft.Azure.Functions.Worker;

using Net.Shared.Persistence.Abstractions.Repositories.NoSql;

using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Core.Persistence.NoSql;
using Telegram.ApAzureBot.Worker.Models;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly Guid _hostId;
    private readonly TelegramCommandStep _step;
    private readonly IPersistenceNoSqlProcessRepository _persistenceProcess;
    private readonly ITelegramCommand _telegramCommand;

    public Functions(IPersistenceNoSqlProcessRepository persistenceProcess, ITelegramCommand telegramCommand)
    {
        var hostId = Environment.GetEnvironmentVariable("HostId");

        ArgumentNullException.ThrowIfNull(hostId, "HostId is not defined");

        if (!Guid.TryParse(hostId, out _hostId))
        {
            throw new ArgumentException("HostId is not valid");
        }
        
        _persistenceProcess = persistenceProcess;
        _telegramCommand = telegramCommand;

        _step = new()
        {
            Id = (int)Core.Constants.TelegramCommandTaskStep.Process,
            Name = Core.Constants.TelegramCommandTaskStep.Process.ToString(),
            Created = DateTime.UtcNow,
            Description = "Process telegram command",
            DocumentVersion = "1.0"
        };
    }

    [Function("TelegramApAzureBotWorker")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TelegramTimer timer)
    {
        var telegramTasks = await _persistenceProcess.GetProcessableData<TelegramCommandTask>(_hostId, _step, 5);

        foreach(var task in telegramTasks)
        {
            task.StatusId = (int)Core.Constants.TelegramCommandTaskStatus.Processing;
        }

        await _persistenceProcess.SetProcessedData(_hostId, _step, null, telegramTasks);

        foreach (var telegramTask in telegramTasks)
        {
            try
            {
                await _telegramCommand.Execute(telegramTask.Message, default);
                telegramTask.StatusId = (int)Core.Constants.TelegramCommandTaskStatus.Completed;
            }
            catch (Exception exception)
            {
                telegramTask.StatusId = (int)Core.Constants.TelegramCommandTaskStatus.Failed;
                telegramTask.Error = exception.Message;
            }
        }

        await _persistenceProcess.SetProcessedData(_hostId, _step, null, telegramTasks);
    }
}
