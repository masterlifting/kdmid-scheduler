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

    public Task Schedule(long chatId, string city, string parameters, CancellationToken cToken)
    {
        var url = _httpClient.BaseAddress + "OrderInfo.aspx?" + parameters;

        var page =
            /*/
            await _httpClient.GetStringAsync(url, cToken);
            /*/
            File.ReadAllText(Environment.CurrentDirectory + "/firstResponse.html");
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
                File.ReadAllBytes(Environment.CurrentDirectory + "/CodeImage.jpeg");
            //*/

            using var stream = new MemoryStream(captcha);

            var photo = Bot.Types.InputFile.FromStream(stream, "captcha.jpeg");

            return _telegramService.Client.SendPhotoAsync(chatId, photo, caption: $"/{Constants.Kdmid}/{city}/captcha?number", cancellationToken: cToken);
        }
    }
    public Task Captcha(long chatId, string city, string parameters, CancellationToken cToken)
    {
        if (parameters.Length < 6)
            throw new NotSupportedException("The captcha is not valid.");

        var requestFormModel = _cache.TryGetValue(chatId, GetRequestFormDataKey(city), out var value)
            ? value!.AsSpan()
            : throw new NotSupportedException("The request form model is not found.");

        var searchString = "\"ctl00$MainContent$txtCode\": \"\",";
        var replacementString = $"\"ctl00$MainContent$txtCode\": \"{parameters}\",";

        int replacementIndex = requestFormModel.IndexOf(searchString);

        if (replacementIndex != -1)
        {
            var newRequestFormModel = new char[requestFormModel.Length + replacementString.Length - searchString.Length];
            requestFormModel[..replacementIndex].CopyTo(newRequestFormModel);
            replacementString.AsSpan().CopyTo(newRequestFormModel.AsSpan(replacementIndex));
            requestFormModel[(replacementIndex + searchString.Length)..].CopyTo(newRequestFormModel.AsSpan(replacementIndex + replacementString.Length));

            requestFormModel = newRequestFormModel.AsSpan();
        }

        var content = new StringContent(requestFormModel.ToString(), Encoding.UTF8, "application/json");

        var response =
            /*/
            await _httpClient.PostAsync(_httpClient.BaseAddress + "OrderInfo.aspx", content, cToken);
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
