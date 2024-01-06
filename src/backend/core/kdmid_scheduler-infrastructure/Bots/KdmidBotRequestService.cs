using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Infrastructure.Bots;

public sealed class KdmidBotRequestService(
    IBotCommandsStore commandsStore,
    IBotResponseService responseService) : IBotRequestService
{
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IBotResponseService _responseService = responseService;

    public async Task OnTextHandler(TextEventArgs args, CancellationToken cToken)
    {
        if (args.Text.Value.StartsWith('/'))
        {
            await _commandsStore.Clear(args.ChatId, cToken);
            await _responseService.CreateResponse(args.ChatId, args.Text.Value.TrimStart('/'), cToken);
        }
        else if (Guid.TryParse(args.Text.Value, out var guid))
        {
            var command = await _commandsStore.Get(args.ChatId, guid, cToken);
            await _responseService.CreateResponse(args.ChatId, command, cToken);
        }
        else
            throw new NotSupportedException($"The message '{args.Text.Value}' is not supported.");
    }
    public Task OnPhotoHandler(PhotoEventArgs photo, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnAudioHandler(AudioEventArgs audio, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnVideoHandler(VideoEventArgs video, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnVoiceHandler(VoiceEventArgs voice, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnDocumentHandler(DocumentEventArgs document, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnLocationHandler(LocationEventArgs location, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
    public Task OnContactHandler(ContactEventArgs contact, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
