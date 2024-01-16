using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;
using KdmidScheduler.Abstractions.Models.Core.v1;

namespace KdmidScheduler.Services;

public sealed class KdmidRequestService(
    IKdmidHttpClient httpClient, 
    IKdmidHtmlDocument htmlDocument, 
    IKdmidCaptcha captchaService) : IKdmidRequestService
{
    private readonly IKdmidHttpClient _httpClient = httpClient;
    private readonly IKdmidCaptcha _captchaService = captchaService;
    private readonly IKdmidHtmlDocument _htmlDocument = htmlDocument;

    public City[] GetSupportedCities(CancellationToken cToken) => new City[]
    {
        new("belgrad", "Belgrade", +1),
        new("budapest", "Budapest", +1),
        new("paris", "Paris", +1),
        new("bucharest", "Bucharest", +2),
        new("riga", "Riga", +2),
        new("sarajevo", "Sarajevo", +1),
        new("tirana", "Tirana", +1),
        new("ljubljana", "Ljubljana", +1),
        new("berlin", "Berlin", +1),
        new("bern", "Bern", +1),
        new("brussels", "Brussels", +1),
        new("dublin", "Dublin", 0),
        new("helsinki", "Helsinki", +2),
        new("hague", "Hague", +1),
    };
    public async Task<AvailableDatesResult> GetAvailableDates(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var startPageResponse = await _httpClient.GetStartPage(city, kdmidId, cToken);
        var startPage = _htmlDocument.GetStartPage(startPageResponse);

        var captchaImage = await _httpClient.GetStartPageCaptchaImage(city, startPage.CaptchaCode, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";
        var captchaValue = await _captchaService.SolveIntegerCaptcha(captchaImage, cToken);

        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaValue}");

        var applicationResponse = await _httpClient.PostApplication(city, kdmidId, startPageFormData, cToken);
        var applicationFormData = _htmlDocument.GetApplicationFormData(applicationResponse);

        var calendarResponse = await _httpClient.PostCalendar(city, kdmidId, applicationFormData, cToken);
        var calendarPage = _htmlDocument.GetCalendarPage(calendarResponse);

        var availableDates = new Dictionary<DateTime, string>(calendarPage.Dates.Count);
        
        foreach (var dateString in calendarPage.Dates)
        {
            if (!DateTime.TryParse(dateString.Value.Split('|')[1], out var parsedDate))
                throw new InvalidOperationException($"The date '{dateString.Value}' is not valid.");

            availableDates.Add(parsedDate, dateString.Value);
        }

        return new AvailableDatesResult(applicationFormData, availableDates);
    }
    public async Task ConfirmChosenDate(City city, KdmidId kdmidId, ChosenDateResult chosenResult, CancellationToken cToken)
    {
        const string ButtonKey = "ctl00%24MainContent%24TextBox1=";
        var buttonValue = Uri.EscapeDataString(chosenResult.ChosenValue);

        var formData = chosenResult.FormData.Replace(ButtonKey, $"{ButtonKey}{buttonValue}");

        var confirmationResponse = await _httpClient.PostConfirmation(city, kdmidId, formData, cToken);

        _htmlDocument.GetConfirmationPage(confirmationResponse);
    }
}
