﻿using KdmidScheduler.Abstractions.Models.Core.v1;

namespace KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;

public interface IKdmidHtmlDocument
{
    StartPage GetStartPage(string payload);
    string GetApplicationFormData(string payload);
    CalendarPage GetCalendarPage(string payload);
    ConfirmationPage GetConfirmationPage(string payload);
}
