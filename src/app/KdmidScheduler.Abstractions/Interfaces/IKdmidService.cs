using KdmidScheduler.Abstractions.Models;

namespace KdmidScheduler.Abstractions.Interfaces;

public interface IKdmidService
{
    City[] GetAvailableCities(CancellationToken cToken);
    Task<AvailableDates> GetAvailableDates(City city, Identifier kdmidId, CancellationToken cToken);
    Task<ConfirmationResult> ConfirmDate(City city, Identifier kdmidId, ChosenDateResult chosenResult, CancellationToken cToken);
}
