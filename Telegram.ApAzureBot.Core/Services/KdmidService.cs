using System.Text;

using HtmlAgilityPack;

using Telegram.ApAzureBot.Core.Abstractions;
using Telegram.Bot;

namespace Telegram.ApAzureBot.Core.Services
{
    public sealed class KdmidService : IKdmidService
    {
        private const string FormDataMediaType = "application/x-www-form-urlencoded";
        private static string GetCaptchaCommand(string city) => $"/{Constants.Kdmid}/{city}/captcha?";
        private static string GetConfirmCommand(string city) => $"/{Constants.Kdmid}/{city}/confirm?";
        private static string GetBaseUrl(string city) => $"https://{city}.{Constants.Kdmid}.ru/queue/";
        private static string GetRequestUrl(string city, string identifier) => GetBaseUrl(city) + "OrderInfo.aspx?" + identifier;
        private static string GetUrlIdentifierKey(string city) => $"{Constants.Kdmid}.{city}.identifier";
        private static string GetRequestFormKey(string city) => $"{Constants.Kdmid}.{city}.request";
        private static string GetResultFormKey(string city) => $"{Constants.Kdmid}.{city}.result";
        private static string GetConfirmValueKey(string city, string key) => $"{Constants.Kdmid}.{city}.confirm.{key}";

        private readonly MemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly IClient _telegramService;

        private readonly Dictionary<string, Func<long, string, string, CancellationToken, Task>> _functions;

        public KdmidService(MemoryCache cache, IHttpClientFactory httpClientFactory, IClient telegramService)
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
                throw new NotSupportedException("Command is not found.");

            var cityIndex = message.IndexOf('/');

            if (cityIndex < 0)
                throw new NotSupportedException("City of the command is not found.");

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
                ? throw new NotSupportedException("Command is not supported.")
                : function(chatId, city.ToString(), parameters.ToString(), cToken);
        }

        public async Task Schedule(long chatId, string city, string urlIdentifier, CancellationToken cToken)
        {
            /*/
            var page = await _httpClient.GetStringAsync(GetRequestUrl(city, urlIdentifier), cToken);
            /*/
            var page = File.ReadAllText(Environment.CurrentDirectory + "/Content/firstResponse.html");
            //*/

            _cache.AddOrUpdate(chatId, GetUrlIdentifierKey(city), urlIdentifier);

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(page);

            string? captchaUrl = null;

            StringBuilder requestFormBuilder = new();

            foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input | //img"))
            {
                if (node.Name == "input")
                {
                    var inputName = node.GetAttributeValue("name", "");
                    var inputValue = node.GetAttributeValue("value", "");

                    var encodedInputName = Uri.EscapeDataString(inputName);
                    var encodedInputValue = Uri.EscapeDataString(inputValue);

                    requestFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
                }
                else if (node.Name == "img")
                {
                    var captchaUrlPart = node.GetAttributeValue("src", "");

                    if (captchaUrlPart.Contains("CodeImage", StringComparison.OrdinalIgnoreCase))
                    {
                        captchaUrl = GetBaseUrl(city) + captchaUrlPart;
                    }
                }
            }

            requestFormBuilder.Remove(0, 1);

            var requestForm = requestFormBuilder.ToString();

            _cache.AddOrUpdate(chatId, GetRequestFormKey(city), requestForm);

            if (captchaUrl is null)
                throw new ArgumentException("Captcha is not found.");
            else
            {
                /*/
                var captcha = await _httpClient.GetByteArrayAsync(captchaUrl, cToken);
                /*/
                var captcha = File.ReadAllBytes(Environment.CurrentDirectory + "/Content/CodeImage.jpeg");
                //*/

                using var stream = new MemoryStream(captcha);

                var photo = Bot.Types.InputFile.FromStream(stream, "captcha.jpeg");

                await _telegramService.Client.SendPhotoAsync(chatId, photo, caption: GetCaptchaCommand(city), cancellationToken: cToken);
            }
        }
        public async Task Captcha(long chatId, string city, string captcha, CancellationToken cToken)
        {
            if (captcha.Length < 6 || !int.TryParse(captcha, out _))
                throw new NotSupportedException("Captcha is not valid.");

            if (!_cache.TryGetValue(chatId, GetUrlIdentifierKey(city), out var urlIdentifier))
                throw new NotSupportedException("URL identifier is not found. Try from the beginning.");

            if (!_cache.TryGetValue(chatId, GetRequestFormKey(city), out var requestForm))
                throw new NotSupportedException("Request data are not found. Try from the beginning.");

            const string OldReplacementString = "ctl00%24MainContent%24txtCode=";
            var newReplacementString = $"{OldReplacementString}{captcha}";

            var requestFormData = requestForm!.Replace(OldReplacementString, newReplacementString);

            /*/
            var content = new StringContent(requestFormData, Encoding.UTF8, FormDataMediaType);
            var requestResponse = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
            var requestPage = await requestResponse.Content.ReadAsStringAsync(cToken);
            /*/
            var requestPage = File.ReadAllText(Environment.CurrentDirectory + "/Content/secondResponse.html");
            //*/

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(requestPage);

            StringBuilder confirmFormBuilder = new();

            foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input"))
            {
                var inputName = node.GetAttributeValue("name", "");
                var inputValue = node.GetAttributeValue("value", "");

                var encodedInputName = Uri.EscapeDataString(inputName);
                var encodedInputValue = Uri.EscapeDataString(inputValue);

                if (!encodedInputName.Equals("ctl00%24MainContent%24ButtonB", StringComparison.OrdinalIgnoreCase))
                {
                    confirmFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
                }
                else
                {
                    confirmFormBuilder.Append($"&{encodedInputName}.x=100");
                    confirmFormBuilder.Append($"&{encodedInputName}.y=20");
                }
            }

            confirmFormBuilder.Remove(0, 1);

            var confirmFormData = confirmFormBuilder.ToString();

            /*/
            content = new StringContent(confirmFormData, Encoding.UTF8, FormDataMediaType);
            var confirmResponse = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
            var confirmPage = await confirmResponse.Content.ReadAsStringAsync(cToken);
            /*/
            var confirmPage = File.ReadAllText(Environment.CurrentDirectory + "/Content/thirdResponse_Ok.html");
            //*/

            htmlDocument.LoadHtml(confirmPage);

            var scheduleTable = htmlDocument.DocumentNode
                .SelectSingleNode("//td[@id='center-panel']")
                .ChildNodes.FirstOrDefault(x => x.Name == "table");

            if (scheduleTable is null)
            {
                await _telegramService.Client.SendTextMessageAsync(chatId, "Making appointments are not found.", cancellationToken: cToken);
            }
            else
            {
                StringBuilder resultFormBuilder = new();

                foreach (var node in htmlDocument.DocumentNode.SelectNodes("//input"))
                {
                    var inputName = node.GetAttributeValue("name", "");
                    var inputValue = node.GetAttributeValue("value", "");

                    var encodedInputName = Uri.EscapeDataString(inputName);
                    var encodedInputValue = Uri.EscapeDataString(inputValue);

                    resultFormBuilder.Append($"&{encodedInputName}={encodedInputValue}");
                }

                resultFormBuilder.Remove(0, 1);

                var resultForm = resultFormBuilder.ToString();

                _cache.AddOrUpdate(chatId, GetResultFormKey(city), resultForm);

                await _telegramService.Client.SendTextMessageAsync(chatId, "Making appointments are found. Choose one of them:", cancellationToken: cToken);

                foreach (var item in scheduleTable.SelectNodes("//input[@type='radio']"))
                {
                    var appointmentValue = item.GetAttributeValue("value", "");

                    var appointmentText = item.NextSibling.InnerText.Trim();

                    _cache.AddOrUpdate(chatId, GetConfirmValueKey(city, appointmentText), appointmentValue);

                    await _telegramService.Client.SendTextMessageAsync(chatId, appointmentText, cancellationToken: cToken);
                }

                await _telegramService.Client.SendTextMessageAsync(chatId, GetConfirmCommand(city), cancellationToken: cToken);
            }
        }
        public async Task Confirm(long chatId, string city, string parameters, CancellationToken cToken)
        {
            if (!_cache.TryGetValue(chatId, GetUrlIdentifierKey(city), out var urlIdentifier))
                throw new NotSupportedException("URL identifier is not found. Try from the beginning.");

            if (!_cache.TryGetValue(chatId, GetConfirmValueKey(city, parameters), out var confirmValue))
                throw new NotSupportedException("Confirm value is not found. Try from the beginning.");

            if (!_cache.TryGetValue(chatId, GetResultFormKey(city), out var resultForm))
                throw new NotSupportedException("Result data are not found. Try from the beginning.");

            var encodedConfirmValue = Uri.EscapeDataString(confirmValue!);

            const string OldReplacementString = "ctl00%24MainContent%24TextBox1=";
            var newReplacementString = $"{OldReplacementString}{encodedConfirmValue}";

            var stringContent = resultForm!.Replace(OldReplacementString, newReplacementString);

            /*/
            var content = new StringContent(stringContent, Encoding.UTF8, FormDataMediaType);
            var response = await _httpClient.PostAsync(GetRequestUrl(city, urlIdentifier!), content, cToken);
            var page = await response.Content.ReadAsStringAsync(cToken);
            /*/
            await _telegramService.Client.SendTextMessageAsync(chatId, "Confirmed.", cancellationToken: cToken);
            //*/

            _cache.Remove(chatId);
        }
    }
}