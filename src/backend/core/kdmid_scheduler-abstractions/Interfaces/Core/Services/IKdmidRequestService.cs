using KdmidScheduler.Abstractions.Models.Core.v1;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidRequestService
{
    City[] GetSupportedCities(CancellationToken cToken);
    Task<AvailableDatesResult> GetAvailableDates(City city, KdmidId kdmidId, CancellationToken cToken);
    Task<ConfirmationResult> ConfirmChosenDate(City city, KdmidId kdmidId, ChosenDateResult chosenResult, CancellationToken cToken);
}
