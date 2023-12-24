using TelegramBot.Abstractions.Models;

namespace TelegramBot.Abstractions.Interfaces.Services.Kdmid;

public interface IKdmidHttpClient
{
    Task<string> GetStartPage(City city, string parameters, CancellationToken cToken);
    Task<byte[]> GetStartPageCaptcha(long chatId, City city, string parameters, CancellationToken cToken);
    Task<string> PostApplication(long chatId, City city, string parameters, string data, CancellationToken cToken);
    Task<string> PostCalendar(City city, string parameters, string data, CancellationToken cToken);
    Task<string> PostConfirmation(long chatId, City city, string id, string data, CancellationToken cToken);
}
