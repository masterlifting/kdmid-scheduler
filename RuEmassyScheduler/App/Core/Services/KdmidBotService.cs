using System.Collections.Immutable;

using Net.Shared.Bots;
using Net.Shared.Bots.Abstractions.Interfaces;

using TelegramBot.Abstractions.Interfaces.Services.Kdmid;

namespace TelegramBot.Services
{
    public sealed class KdmidBotService(IKdmidService service, IBotCommandProvider commandProvider) : IBotService
    {
        private readonly IKdmidService _service = service;
        private readonly IBotCommandProvider _commandProvider = commandProvider;

        public Task HandleText(string chatId, string text)
        {
            if(text.StartsWith('/'))
            {
                return _service.HandleCommand(chatId, new BotCommand(text));
            }
            else if (Guid.TryParse(text, out var guid))
            {
                if(!_commandProvider.TryGetCommand(chatId, guid, out string commandText))
                    throw new InvalidOperationException("The command is not found.");

                return _service.HandleCommand(chatId, new BotCommand(commandText));
            }
            else
            {
                throw new NotSupportedException("The message is not supported.");
            }
        }
        public Task HandlePhoto(string chatId, ImmutableArray<(string FileId, long? FileSize)> immutableArray)
        {
            throw new NotImplementedException();
        }
        public Task HandleVideo(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName)
        {
            throw new NotImplementedException();
        }
        public Task HandleAudio(string chatId, string fileId, long? fileSize, string? mimeType, string? title)
        {
            throw new NotImplementedException();
        }
        public Task HandleVoice(string chatId, string fileId, long? fileSize, string? mimeType)
        {
            throw new NotImplementedException();
        }
        public Task HandleDocument(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName)
        {
            throw new NotImplementedException();
        }
        public Task HandleLocation(string chatId, double latitude, double longitude)
        {
            throw new NotImplementedException();
        }
        public Task HandleContact(string chatId, string phoneNumber, string firstName, string? lastName)
        {
            throw new NotImplementedException();
        }
    }
}
