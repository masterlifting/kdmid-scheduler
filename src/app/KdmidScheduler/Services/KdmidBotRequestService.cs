using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotRequestService(IBotCommandsStore commandsStore,IBotResponseService responseService) : IBotRequestService
{
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IBotResponseService _responseService = responseService;

    public async Task OnTextHandler(TextEventArgs args)
    {
        BotCommand command;

        if (args.Text.Value.StartsWith('/'))
            command = new BotCommand(args.Text.Value);
        else if (Guid.TryParse(args.Text.Value, out var guid))
            command = await _commandsStore.GetCommand(args.ChatId, guid, CancellationToken.None);
        else
            throw new NotSupportedException($"The message '{args.Text.Value}' is not supported.");

        await _responseService.CreateResponse(args.ChatId, command, CancellationToken.None);
    }
    public Task OnPhotoHandler(PhotoEventArgs photo)
    {
        throw new NotImplementedException();
    }
    public Task OnAudioHandler(AudioEventArgs audio)
    {
        throw new NotImplementedException();
    }
    public Task OnVideoHandler(VideoEventArgs video)
    {
        throw new NotImplementedException();
    }
    public Task OnVoiceHandler(VoiceEventArgs voice)
    {
        throw new NotImplementedException();
    }
    public Task OnDocumentHandler(DocumentEventArgs document)
    {
        throw new NotImplementedException();
    }
    public Task OnLocationHandler(LocationEventArgs location)
    {
        throw new NotImplementedException();
    }
    public Task OnContactHandler(ContactEventArgs contact)
    {
        throw new NotImplementedException();
    }
}
