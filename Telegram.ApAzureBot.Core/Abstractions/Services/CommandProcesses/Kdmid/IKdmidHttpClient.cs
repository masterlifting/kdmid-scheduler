﻿using Telegram.ApAzureBot.Core.Models.CommandProcesses.Kdmid;

namespace Telegram.ApAzureBot.Core.Abstractions.Services.CommandProcesses.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(KdmidCity city, string parameters, CancellationToken cToken);
    Task<byte[]> GetStartPageCaptcha(long chatId, KdmidCity city, string parameters, CancellationToken cToken);
    Task<string> PostApplication(long chatId, KdmidCity city, string parameters, string data, CancellationToken cToken);
    Task<string> PostCalendar(KdmidCity city, string parameters, string data, CancellationToken cToken);
    Task<string> PostConfirmation(long chatId, KdmidCity city, string id, string data, CancellationToken cToken);
}
