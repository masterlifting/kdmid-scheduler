using System.Text;
using System.Text.Json;

using Telegram.ApAzureBot.Core.Abstractions.Services.Web.Captcha;

namespace Telegram.ApAzureBot.Infrastructure.Services.Web.Captcha;

public sealed class AntiCaptchaService : ICaptchaService
{
    private readonly string _apiKey;
    readonly HttpClient _httpClient;
    
    public AntiCaptchaService(IHttpClientFactory httpClientFactory)
    {
        var apiKey = Environment.GetEnvironmentVariable("AntiCaptchaKey", EnvironmentVariableTarget.Process);

        ArgumentNullException.ThrowIfNull(apiKey, "AntiCaptchaKey is not set");

        _apiKey = apiKey;

        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<ushort> SolveInteger(byte[] img, CancellationToken cToken)
    {
        var content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"task\": {{\"type\": \"ImageToTextTask\", \"body\": \"{Convert.ToBase64String(img)}\", \"phrase\": false, \"case\": false, \"numeric\": true, \"math\": 0, \"minLength\": 1, \"maxLength\": 1}}}}", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("https://api.anti-captcha.com/createTask", content, cToken);
        var responseContent = await response.Content.ReadAsStringAsync(cToken);
        var responseContentResult = JsonSerializer.Deserialize<dynamic>(responseContent);
        //ValueKind = Object : "{"errorId":0,"taskId":312407635}"
        string taskId = responseContentResult.taskId;
        var status = "processing";

        content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"taskId\": \"{taskId}\"}}", Encoding.UTF8, "application/json");

        while (status == "processing")
        {
            await Task.Delay(1000, cToken);

            response = await _httpClient.PostAsync("https://api.anti-captcha.com/getTaskResult", content, cToken);
            responseContent = await response.Content.ReadAsStringAsync(cToken);
            responseContentResult = JsonSerializer.Deserialize<dynamic>(responseContent);

            status = responseContentResult?["status"].Value<string>();

            if (status == "ready")
            {
                var solution = JsonSerializer.Deserialize<dynamic>(responseContent)?["solution"];
                var result = solution?["text"].Value<ushort>();
                return result;
            }
        }

        throw new NotSupportedException("Captcha solving failed");
    }
}
