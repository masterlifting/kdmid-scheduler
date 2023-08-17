using Microsoft.Azure.Functions.Worker;
using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Abstractions.Services.Telegram;
using Telegram.ApAzureBot.Worker.Models;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Telegram.ApAzureBot.Worker;

public class Functions
{
    private readonly ITelegramCommand _command;
    private readonly ITelegramCommandTaskRepository _repository;

    public Functions(ITelegramCommandTaskRepository repository, ITelegramCommand command)
    {
        _repository = repository;
        _command = command;
    }

    [Function("TelegramApAzureBotWorker")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TelegramTimer timer)
    {
        var telegramTasks = await _repository.GetReadyTasks(5);

        foreach (var telegramTask in telegramTasks)
        {
            try
            {
                await _command.Execute(new(telegramTask.ChatId, telegramTask.Text), default);
                telegramTask.StatusId = (int)ProcessStatuses.Completed;
            }
            catch (Exception exception)
            {
                telegramTask.StatusId = (int)ProcessStatuses.Error;
                telegramTask.Error = exception.Message;
            }
        }

        await _repository.UpdateTaskStatus(telegramTasks);
    }
}
