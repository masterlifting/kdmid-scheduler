using KdmidScheduler.Abstractions.Interfaces.Core.Services;
using KdmidScheduler.Abstractions.Interfaces.Infrastructure.Web;
using KdmidScheduler.Abstractions.Models.Core.v1.Kdmid;

using Microsoft.Extensions.Logging;

using Net.Shared.Extensions.Logging;

namespace KdmidScheduler.Services;

public sealed class KdmidRequestService(
    ILogger<KdmidRequestService> logger,
    IKdmidRequestHttpClient httpClient,
    IKdmidRequestHtmlDocument htmlDocument,
    IKdmidRequestCaptcha captchaService) : IKdmidRequestService
{
    private readonly ILogger<KdmidRequestService> _log = logger;
    private readonly IKdmidRequestHttpClient _httpClient = httpClient;
    private readonly IKdmidRequestCaptcha _captchaService = captchaService;
    private readonly IKdmidRequestHtmlDocument _htmlDocument = htmlDocument;

    private static readonly Dictionary<string, City> SupportedCities = new(StringComparer.OrdinalIgnoreCase)
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
        { "podgorica", new("podgorica", "Podgorica", +1) }
    };

    public City[] GetSupportedCities(CancellationToken cToken) => SupportedCities.Values.OrderBy(x => x.Name).ToArray();
    public City GetSupportedCity(string cityCode, CancellationToken cToken)
    {
        if (!SupportedCities.TryGetValue(cityCode, out var city))
            throw new InvalidOperationException($"The city '{cityCode}' is not supported.");

        return city;
    }
    public async Task<AvailableDatesResult> GetAvailableDates(City city, KdmidId kdmidId, CancellationToken cToken)
    {
        var startPageResponse = await _httpClient.GetStartPage(city, kdmidId, cToken);
        var startPage = _htmlDocument.GetStartPage(startPageResponse);

        var captchaImage = await _httpClient.GetCaptchaImage(city, kdmidId, startPage.CaptchaCode, cToken);
        var captchaValue = await _captchaService.SolveIntegerCaptcha(captchaImage, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";
        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaValue}");

        var applicationResponse = await _httpClient.PostApplication(city, kdmidId, startPageFormData, cToken);
        var applicationFormData = _htmlDocument.GetApplicationFormData(applicationResponse);

        var calendarResponse = await _httpClient.PostCalendar(city, kdmidId, applicationFormData, cToken);
        var calendarPage = _htmlDocument.GetCalendarPage(calendarResponse);

        var availableDates = new Dictionary<string, string>(calendarPage.Dates.Count);

        foreach (var date in calendarPage.Dates)
        {
            if (!DateTime.TryParse(date.Value.Split('|')[1], out var _))
                throw new InvalidOperationException($"The date '{date.Value}' for the '{city.Name}' with the Id '{kdmidId.Id}' is not valid.");

            if(availableDates.ContainsKey(date.Key))
            {
                _log.Warn($"The date '{date.Key}' for the '{city.Name}' with the Id '{kdmidId.Id}' is already added to the available dates list.");
                continue;
            }
            else
                availableDates.Add(date.Key, date.Value);
        }

        return new AvailableDatesResult(calendarPage.FormData, availableDates);
    }
    public async Task ConfirmChosenDate(City city, KdmidId kdmidId, ChosenDateResult chosenResult, CancellationToken cToken)
    {
        const string ButtonKey = "ctl00%24MainContent%24TextBox1=";
        var buttonValue = Uri.EscapeDataString(chosenResult.ChosenValue);

        var formData = chosenResult.FormData.Replace(ButtonKey, $"{ButtonKey}{buttonValue}");

        var confirmationResponse = await _httpClient.PostConfirmation(city, kdmidId, formData, cToken);

        _ =_htmlDocument.GetConfirmationResult(confirmationResponse);
    }
}
