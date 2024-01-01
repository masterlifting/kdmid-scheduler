using KdmidScheduler.Abstractions.Interfaces;
using KdmidScheduler.Abstractions.Models.v1;

namespace KdmidScheduler.Services;

public sealed class KdmidService(
    IKdmidHttpClient httpClient, 
    IKdmidHtmlDocument htmlDocument, 
    IKdmidCaptcha captchaService) : IKdmidService
{
    private readonly IKdmidHttpClient _httpClient = httpClient;
    private readonly IKdmidCaptcha _captchaService = captchaService;
    private readonly IKdmidHtmlDocument _htmlDocument = htmlDocument;

    public City[] GetAvailableCities(CancellationToken cToken) => new City[]
    {
        new("belgrad", "Belgrade"),
        new("budapest", "Budapest"),
        new("paris", "Paris"),
        new("bucharest", "Bucharest"),
        new("riga", "Riga"),
        new("sarajevo", "Sarajevo"),
        new("tirana", "Tirana"),
        new("ljubljana", "Ljubljana"),
        new("berlin", "Berlin"),
        new("bern", "Bern"),
        new("brussels", "Brussels"),
        new("dublin", "Dublin"),
        new("helsinki", "Helsinki"),
        new("hague", "Hague")
    };
    public async Task<AvailableDates> GetAvailableDates(City city, Identifier identifier, CancellationToken cToken)
    {
        var startPageResponse = await _httpClient.GetStartPage(city, identifier, cToken);
        var startPage = _htmlDocument.GetStartPage(startPageResponse);

        var captchaImage = await _httpClient.GetStartPageCaptchaImage(city, startPage.CaptchaCode, cToken);

        const string CaptchaKey = "ctl00%24MainContent%24txtCode=";
        var captchaValue = await _captchaService.SolveIntegerCaptcha(captchaImage, cToken);

        var startPageFormData = startPage.FormData.Replace(CaptchaKey, $"{CaptchaKey}{captchaValue}");

        var applicationResponse = await _httpClient.PostApplication(city, identifier, startPageFormData, cToken);
        var applicationFormData = _htmlDocument.GetApplicationFormData(applicationResponse);

        var calendarResponse = await _httpClient.PostCalendar(city, identifier, applicationFormData, cToken);
        var calendarPage = _htmlDocument.GetCalendarPage(calendarResponse);

        var availableDates = new Dictionary<DateTime, string>(calendarPage.Variants.Count);
        
        foreach (var variant in calendarPage.Variants)
        {
            if (!DateTime.TryParse(variant.Value.Split('|')[1], out var date))
            {
                throw new InvalidOperationException($"The date '{variant.Value}' is not valid.");
            }

            availableDates.Add(date, variant.Value);
        }

        return new AvailableDates(applicationFormData, availableDates);
    }
    public async Task<ConfirmationResult> ConfirmDate(City city, Identifier identifier, ChosenDateResult chosenResult, CancellationToken cToken)
    {
        const string ButtonKey = "ctl00%24MainContent%24TextBox1=";
        var buttonValue = Uri.EscapeDataString(chosenResult.ChosenValue);

        var formData = chosenResult.FormData.Replace(ButtonKey, $"{ButtonKey}{buttonValue}");

        var confirmationResponse = await _httpClient.PostConfirmation(city, identifier, formData, cToken);

        var confirmation = _htmlDocument.GetConfirmationPage(confirmationResponse);

        if(confirmation.Result.StartsWith("/"))
            return new ConfirmationResult(false, confirmation.Result);

        return new ConfirmationResult(true, confirmation.Result);
    }
}
