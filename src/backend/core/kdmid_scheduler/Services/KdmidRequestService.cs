using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Services;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

namespace KdmidScheduler.Services;

public sealed class KdmidRequestService(
    IKdmidHttpClient httpClient, 
    IKdmidHtmlDocument htmlDocument, 
    IKdmidCaptcha captchaService) : IKdmidRequestService
{
    private readonly IKdmidHttpClient _httpClient = httpClient;
    private readonly IKdmidCaptcha _captchaService = captchaService;
    private readonly IKdmidHtmlDocument _htmlDocument = htmlDocument;

    private static readonly Dictionary<string, City> _supportedCities = new(StringComparer.OrdinalIgnoreCase)
    {
        { "belgrad", new("belgrad", "Belgrade", +1) },
        { "budapest", new("budapest", "Budapest", +1) },
        { "paris", new("paris", "Paris", +1) },
        { "bucharest", new("bucharest", "Bucharest", +2) },
        { "riga", new("riga", "Riga", +2) },
        { "sarajevo", new("sarajevo", "Sarajevo", +1) },
        { "tirana", new("tirana", "Tirana", +1) },
        { "ljubljana", new("ljubljana", "Ljubljana", +1) },
        { "berlin", new("berlin", "Berlin", +1) },
        { "bern", new("bern", "Bern", +1) },
        { "brussels", new("brussels", "Brussels", +1) },
        { "dublin", new("dublin", "Dublin", 0) },
        { "helsinki", new("helsinki", "Helsinki", +2) },
        { "hague", new("hague", "Hague", +1) },
    };

    public City[] GetSupportedCities(CancellationToken cToken) => _supportedCities.Values.ToArray();
    public City GetSupportedCity(string cityCode, CancellationToken cToken)
    {
        if (!_supportedCities.TryGetValue(cityCode, out var city))
            throw new InvalidOperationException($"The city '{cityCode}' is not supported.");

        return city;
    }
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
