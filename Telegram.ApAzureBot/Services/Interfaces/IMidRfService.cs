namespace Telegram.ApAzureBot.Services.Interfaces;

public interface IMidRfService
{
    Task<byte[]> CheckSchedule(string message);
    Task<string> SetCaptcha(string message);
    Task<string> Schedule(string message);
}
