using System.Collections.Immutable;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services
{
    public sealed class KdmidBotRequestService(
        IBotCommandsStore commandsStore,
        IBotResponseService responseService) : IBotRequestService
    {
        private readonly IBotCommandsStore _commandsStore = commandsStore;
        private readonly IBotResponseService _responseService = responseService;

        public async Task HandleText(string chatId, string text, CancellationToken cToken)
        {
            BotCommand command;

            if (text.StartsWith('/'))
                command = new BotCommand(text);
            else if (Guid.TryParse(text, out var guid))
                command = await _commandsStore.GetCommand(chatId, guid, cToken);
            else
                throw new NotSupportedException($"The message '{text}' is not supported.");

            await _responseService.CreateResponse(chatId, command, cToken);
        }
        public Task HandlePhoto(string chatId, ImmutableArray<(string FileId, long? FileSize)> photos, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleVideo(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleAudio(string chatId, string fileId, long? fileSize, string? mimeType, string? title, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleVoice(string chatId, string fileId, long? fileSize, string? mimeType, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleDocument(string chatId, string fileId, long? fileSize, string? mimeType, string? fileName, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleLocation(string chatId, double latitude, double longitude, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
        public Task HandleContact(string chatId, string phoneNumber, string firstName, string? lastName, CancellationToken cToken)
        {
            throw new NotImplementedException();
        }
    }
}
