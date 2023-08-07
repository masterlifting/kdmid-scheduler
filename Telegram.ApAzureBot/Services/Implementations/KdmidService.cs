using System.Net;
using System.Text;

using HtmlAgilityPack;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class KdmidService : IKdmidService
{
    private static string GetIdentifierKey(string city) => $"{Constants.Kdmid}.{city}.identifier";
    private static string GetRequestFormDataKey(string city) => $"{Constants.Kdmid}.{city}.requestFormData";

    private readonly MemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ITelegramService _telegramService;

    private readonly Dictionary<string, Func<long, string, string, CancellationToken, Task>> _functions;

    public KdmidService(MemoryCache cache, IHttpClientFactory httpClientFactory, ITelegramService telegramService)
    {
        _cache = cache;
        _telegramService = telegramService;

        _httpClient = httpClientFactory.CreateClient(Constants.Kdmid);

        _functions = new()
        {
            { "schedule", Schedule },
            { "captcha", Captcha },
            { "confirm", Confirm },
        };
    }

    public Task Process(long chatId, ReadOnlySpan<char> message, CancellationToken cToken)
    {
        if (message.Length == 0)
            throw new NotSupportedException("The command is not found.");

        var cityIndex = message.IndexOf('/');

        if (cityIndex < 0)
            throw new NotSupportedException("The city for the command is not found.");

        var city = message[0..cityIndex];

        message = message[(cityIndex + 1)..];

        var nextCommandStartIndex = message.IndexOf('?');

        var command = nextCommandStartIndex > 0
            ? message[0..nextCommandStartIndex]
            : message;

        var parameters = nextCommandStartIndex > 0
            ? message[(nextCommandStartIndex + 1)..]
            : string.Empty;

        return !_functions.TryGetValue(command.ToString(), out var function)
            ? throw new NotSupportedException("The process of the command is not supported.")
            : function(chatId, city.ToString(), parameters.ToString(), cToken);
    }

    public async Task Schedule(long chatId, string city, string parameters, CancellationToken cToken)
    {
        var url = _httpClient.BaseAddress + "OrderInfo.aspx?" + parameters;

        var page =
            /*/
            await _httpClient.GetStringAsync(url, cToken);
            /*/
            File.ReadAllText(Environment.CurrentDirectory + "/Content/firstResponse.html");
            //*/

        _cache.AddOrUpdate(chatId, GetIdentifierKey(city), parameters);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(page);

        string? captchaUrl = null;

        StringBuilder requestFormModelBuilder = new();

        requestFormModelBuilder.Append('{');

        foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input | //img"))
        {
            if (node.Name == "input")
            {
                string inputName = node.GetAttributeValue("name", "");
                string inputValue = node.GetAttributeValue("value", "");

                requestFormModelBuilder.Append($"\"{inputName}\": \"{inputValue}\",");
            }
            else if (node.Name == "img")
            {
                string captchaUrlPart = node.GetAttributeValue("src", "");

                if (captchaUrlPart.Contains("CodeImage", StringComparison.OrdinalIgnoreCase))
                {
                    captchaUrl = _httpClient.BaseAddress + captchaUrlPart;
                }
            }
        }

        requestFormModelBuilder.Append('}');

        var requestFormModel = requestFormModelBuilder.ToString();

        _cache.AddOrUpdate(chatId, GetRequestFormDataKey(city), requestFormModel);

        if (captchaUrl is null)
            throw new NotSupportedException("The captcha url is null.");
        else
        {
            var captcha =
                /*/
                await _httpClient.GetByteArrayAsync(captchaUrl, cToken);
                /*/
                File.ReadAllBytes(Environment.CurrentDirectory + "/Content/CodeImage.jpeg");
                //*/

            using var stream = new MemoryStream(captcha);

            var photo = Bot.Types.InputFile.FromStream(stream, "captcha.jpeg");

            await _telegramService.Client.SendPhotoAsync(chatId, photo, caption: $"/{Constants.Kdmid}/{city}/captcha?number", cancellationToken: cToken);
        }
    }
    public async Task Captcha(long chatId, string city, string parameters, CancellationToken cToken)
    {
        if (parameters.Length < 6)
            throw new NotSupportedException("The captcha is not valid.");

        if(!_cache.TryGetValue(chatId, GetRequestFormDataKey(city), out var requestFormModel))
            throw new NotSupportedException("The request form model is not found. Try from the beginning.");

        var searchString = "\"ctl00$MainContent$txtCode\": \"\",";
        var replacementString = $"\"ctl00$MainContent$txtCode\": \"{parameters}\",";

        var stringContent = requestFormModel!.Replace(searchString, replacementString);

        //*/
        var content = new StringContent(stringContent, Encoding.UTF8, "application/x-www-form-urlencoded");

        if(!_cache.TryGetValue(chatId, GetIdentifierKey(city), out var requestIdentifier))
            throw new NotSupportedException("The identifier is not found. Try from the beginning.");

        var request = new HttpRequestMessage(HttpMethod.Post, _httpClient.BaseAddress + "OrderInfo.aspx?" + requestIdentifier);
        
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
        request.Headers.Add("Accept-Language", "en,ru;q=0.9");
        request.Headers.Add("Cache-Control", "max-age=0");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("DNT", "1");
        request.Headers.Add("Host", "belgrad.kdmid.ru");
        request.Headers.Add("Origin", "https://belgrad.kdmid.ru");
        request.Headers.Add("Referer", $"https://belgrad.kdmid.ru/queue/OrderInfo.aspx?{requestIdentifier}");
        request.Headers.Add("Sec-Fetch-Dest", "document");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36 Edg/115.0.1901.188");
        request.Headers.Add("sec-ch-ua", "\"Not/A)Brand\";v=\"99\", \"Microsoft Edge\";v=\"115\", \"Chromium\";v=\"115\"");
        request.Headers.Add("sec-ch-ua-mobile", "?0");
        request.Headers.Add("sec-ch-ua-platform", "Windows");

        request.Content = content;

        var response = await _httpClient.SendAsync(request, cToken);

        var page = await response.Content.ReadAsStringAsync();
        /*/
        new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
        //*/

        throw new NotImplementedException();
    }
    public Task Confirm(long chatId, string city, string parameters, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
