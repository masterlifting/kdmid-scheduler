using System.Net.Http;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class MidRfService : IMidRfService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _serbianMidRfSecretQueryParameters;

    public MidRfService(ILogger<MidRfService> logger, IHttpClientFactory httpClientFactory)
    {
        var serbianMidRfSecretQueryParameters = Environment.GetEnvironmentVariable("SerbianMidRfSecretQueryParameters", EnvironmentVariableTarget.Process);

        ArgumentNullException.ThrowIfNull(serbianMidRfSecretQueryParameters);

        _serbianMidRfSecretQueryParameters = serbianMidRfSecretQueryParameters;

        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CheckSerbianMidRf(string[] parameters)
    {
        var client = _httpClientFactory.CreateClient(Constants.MidRfHttpClientName);

        if(parameters.Length == 0)
            return ProcessCaptcha(client, client.BaseAddress + _serbianMidRfSecretQueryParameters);

        //await Task.Delay(10000);

        var captcha =
            //await client.GetByteArrayAsync(client.BaseAddress + captchaQueryParameter);
            File.ReadAllBytes(Environment.CurrentDirectory + "/CodeImage.jpeg");

        var captchaResult = "123456";

        //File.WriteAllBytes(Environment.CurrentDirectory + "/captcha.jpeg", captcha);

        //First Post,200

        //Second Post -> 302 - нет мест, 200 - есть места

        return "The function is not implemented yet.";
    }

    private async Task<byte[]> ProcessCaptcha(HttpClient httpClient)
    {
        var page =
            //await client.GetStringAsync(client.BaseAddress + _serbianMidRfSecretQueryParameters);
            File.ReadAllText(Environment.CurrentDirectory + "/firstResponse.html");

        var htmlDocument = new HtmlDocument();

        htmlDocument.LoadHtml(page);

        var captchaQueryParameter = htmlDocument.DocumentNode
            .SelectNodes("//img")
            .Where(x => x is not null)
            .Select(x => x.GetAttributeValue("src", ""))
            .FirstOrDefault(x => x.Contains("CodeImage", StringComparison.OrdinalIgnoreCase));

        ArgumentNullException.ThrowIfNull(captchaQueryParameter);

        var captcha =
            //await client.GetByteArrayAsync(url);
            File.ReadAllBytes(Environment.CurrentDirectory + "/CodeImage.jpeg");

        return captcha;
    }

    public sealed class FormDataFirstModel
    {
        public string? __EVENTTARGET { get; set; }
        public string? __EVENTARGUMENT { get; set; }
        public string __VIEWSTATE { get; set; } = null!;
        public string __EVENTVALIDATION { get; set; } = null!;
        public string ctl00_MainContent_txtID { get; set; } = null!;
        public string ctl00_MainContent_txtUniqueID { get; set; } = null!;
        public string ctl00_MainContent_txtCode { get; set; } = null!;
        public string ctl00_MainContent_ButtonA { get; set; } = null!;
    }
    public class FormDataSecondModel
    {
        public string? __EVENTTARGET { get; set; }
        public string? __EVENTARGUMENT { get; set; }
        public string __VIEWSTATE { get; set; } = null!;
        public string __EVENTVALIDATION { get; set; } = null!;
        public string ctl00_MainContent_ButtonB_x { get; set; } = "143";
        public string ctl00_MainContent_ButtonB_y { get; set; } = "26";
    }

}
