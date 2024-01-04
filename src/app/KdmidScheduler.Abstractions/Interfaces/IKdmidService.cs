﻿using KdmidScheduler.Abstractions.Models.v1;

namespace KdmidScheduler.Abstractions.Interfaces;

public interface IKdmidService
{
    City[] GetSupportedCities(CancellationToken cToken);
    Task<AvailableDates> GetAvailableDates(City city, Identifier kdmidId, CancellationToken cToken);
    Task<ConfirmationResult> ConfirmDate(City city, Identifier kdmidId, ChosenDateResult chosenResult, CancellationToken cToken);
}
