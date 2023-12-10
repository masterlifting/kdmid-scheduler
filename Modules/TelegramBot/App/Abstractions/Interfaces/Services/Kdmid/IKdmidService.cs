using TelegramBot.Abstractions.Models;

namespace TelegramBot.Abstractions.Interfaces.Services.Kdmid;

public interface IKdmidService
{
    Task<IReadOnlyCollection<Embassy>> GetAvailableEmbassies(CancellationToken cToken);
    Task GetAvailableBookings(Embassy embassy, CancellationToken cToken);
    Task ConfirmChosenBooking(Embassy command, CancellationToken cToken);
    Task SetAutoSeekAvailableBookingsForEmbassy(Embassy command, CancellationToken cToken);
}
