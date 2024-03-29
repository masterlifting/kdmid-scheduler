﻿using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Abstractions.Interfaces.Core.Services;

public interface IKdmidRequestService
{
    City[] GetSupportedCities(CancellationToken cToken);
    City GetSupportedCity(string cityCode, CancellationToken cToken);
    Task<AvailableDatesResult> GetAvailableDates(City city, KdmidId kdmidId, CancellationToken cToken);
    Task ConfirmChosenDate(City city, KdmidId kdmidId, ChosenDateResult chosenResult, CancellationToken cToken);
}
