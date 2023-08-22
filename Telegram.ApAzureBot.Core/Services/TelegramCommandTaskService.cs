using Telegram.ApAzureBot.Core.Abstractions.Persistence.Repositories;
using Telegram.ApAzureBot.Core.Abstractions.Services;

using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Telegram.ApAzureBot.Core.Services
{
    public sealed class TelegramCommandTaskService : ITelegramCommandTaskService
    {
        private readonly ITelegramCommand _command;
        private readonly ITelegramCommandTaskRepository _repository;

        public TelegramCommandTaskService(ITelegramCommandTaskRepository repository, ITelegramCommand command)
        {
            _repository = repository;
            _command = command;
        }

        public async Task Process()
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
}
