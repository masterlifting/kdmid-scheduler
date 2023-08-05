using System.Net;
using System.Net.Http;
using System.Text;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;
using Telegram.Bot;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class MidRfService : IMidRfService
{
    private const string IdentifierKey = "midrf.identifier";
    private const string RequestFormModelKey = "midrf.requestFormModel";

    private readonly ILogger _logger;
    private readonly MemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly ITelegramService _telegramService;

    private readonly Dictionary<string, Func<long, string, CancellationToken, Task>> _functions;

    public MidRfService(ILogger<MidRfService> logger, MemoryCache cache, IHttpClientFactory httpClientFactory, ITelegramService telegramService)
    {
        _logger = logger;
        _cache = cache;
        _telegramService = telegramService;

        _httpClient = httpClientFactory.CreateClient(Constants.MidRfHttpClientName);

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
            throw new NotSupportedException("The message for midrf is empty.");

        var nextCommandStartIndex = message.IndexOf('?');

        var command = nextCommandStartIndex > 0
            ? message[0..nextCommandStartIndex]
            : message;

        var parameters = nextCommandStartIndex > 0
            ? message[(nextCommandStartIndex + 1)..]
            : string.Empty;

        return !_functions.TryGetValue(command.ToString(), out var function)
            ? throw new NotSupportedException("The midrf function is not supported.")
            : function(chatId, parameters.ToString(), cToken);
    }

    public Task Schedule(long chatId, string parameters, CancellationToken cToken)
    {
        var url = _httpClient.BaseAddress + "OrderInfo.aspx?" + parameters;

        var page =
            /*/
            await _httpClient.GetStringAsync(url, cToken);
            /*/
            File.ReadAllText(Environment.CurrentDirectory + "/firstResponse.html");
            //*/

        _cache.AddOrUpdate(chatId, IdentifierKey, parameters);

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

        _cache.AddOrUpdate(chatId, RequestFormModelKey, requestFormModel);

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

            var photo = Bot.Types.InputFile.FromStream(stream, "Solve the captcha");

            return _telegramService.Bot.SendPhotoAsync(chatId, photo, cancellationToken: cToken);
        }
    }
    public Task Captcha(long chatId, string parameters, CancellationToken cToken)
    {
        if (parameters.Length < 7)
            throw new NotSupportedException("The captcha is not recognized.");

        var requestFormModel = _cache.TryGetValue(chatId, RequestFormModelKey, out var value)
            ? value!
            : throw new NotSupportedException("The request form model is not found.");

        requestFormModel = requestFormModel.Replace("\"ctl00$MainContent$txtCode\": \"\",", $"\"ctl00$MainContent$txtCode\": \"{parameters}\",");

        var content = new StringContent(requestFormModel, Encoding.UTF8, "application/json");

        var response =
            /*/
            await _httpClient.PostAsync(_httpClient.BaseAddress + "OrderInfo.aspx", content, cToken);
            /*/    
            new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };
            //*/
        
        throw new NotImplementedException();
    }
    public Task Confirm(long chatId, string parameters, CancellationToken cToken)
    {
        throw new NotImplementedException();
    }
}
