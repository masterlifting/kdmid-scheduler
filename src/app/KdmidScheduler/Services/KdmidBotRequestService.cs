using Microsoft.Extensions.Logging;

using Net.Shared.Bots.Abstractions.Interfaces;
using Net.Shared.Bots.Abstractions.Models;

namespace KdmidScheduler.Services;

public sealed class KdmidBotRequestService(
    ILogger<KdmidBotRequestService> logger, 
    IBotCommandsStore commandsStore,
    IBotResponseService responseService) : IBotRequestService
{
    private readonly ILogger<KdmidBotRequestService> _log = logger;
    private readonly IBotCommandsStore _commandsStore = commandsStore;
    private readonly IBotResponseService _responseService = responseService;

    public async Task OnTextHandler(TextEventArgs args)
    {
        if (args.Text.Value.StartsWith('/'))
            await _responseService.CreateResponse(args.ChatId, args.Text.Value.TrimStart('/'), CancellationToken.None);
        else if (Guid.TryParse(args.Text.Value, out var guid))
        {
            var command = await _commandsStore.Get(args.ChatId, guid, CancellationToken.None);
            await _responseService.CreateResponse(args.ChatId, command, CancellationToken.None);
        }
        else
            throw new NotSupportedException($"The message '{args.Text.Value}' is not supported.");
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
