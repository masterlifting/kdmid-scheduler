using KdmidScheduler.Abstractions.Models.Core.v1;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidRequestService
{
    City[] GetSupportedCities(CancellationToken cToken);
    Task<AvailableDatesResult> GetAvailableDates(City city, Identifier kdmidId, CancellationToken cToken);
    Task<ConfirmationResult> ConfirmChosenDate(City city, Identifier kdmidId, ChosenDateResult chosenResult, CancellationToken cToken);
}
