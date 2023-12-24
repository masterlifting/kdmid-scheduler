using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Configuration;

namespace TelegramBot.Infrastructure.Services.CommandProcesses.Kdmid;

public sealed class KdmidCaptchaService : IKdmidCaptchaService
{
    private readonly string _apiKey;
    readonly HttpClient _httpClient;

    public KdmidCaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        var apiKey = configuration["AntiCaptchaKey"];

        ArgumentNullException.ThrowIfNull(apiKey, "AntiCaptchaKey is not set");

        _apiKey = apiKey;

        _httpClient = httpClientFactory.CreateClient(Core.Constants.AntiCaptcha);
    }

    public async Task<string> SolveInteger(byte[] img, CancellationToken cToken)
    {
        try
        {
            var captcha = Convert.ToBase64String(img);
            var content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"task\": {{\"type\": \"ImageToTextTask\", \"body\": \"{captcha}\", \"phrase\": false, \"case\": false, \"numeric\": true, \"math\": 0, \"minLength\": 1, \"maxLength\": 1}}}}", Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_httpClient.BaseAddress + "createTask", content, cToken);
            var responseContent = await response.Content.ReadAsStringAsync(cToken);

            var taskId = JsonSerializer.Deserialize<Dictionary<string, int>>(responseContent)!["taskId"];

            var status = "processing";

            content = new StringContent($"{{\"clientKey\": \"{_apiKey}\", \"taskId\": \"{taskId}\"}}", Encoding.UTF8, "application/json");

            while (status == "processing")
            {
                await Task.Delay(500, cToken);

                response = await _httpClient.PostAsync(_httpClient.BaseAddress + "getTaskResult", content, cToken);
                responseContent = await response.Content.ReadAsStringAsync(cToken);

                var taskResult = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

                status = taskResult!["status"].ToString();

                if (status == "ready")
                {
                    var resultContent = taskResult!["solution"]?.ToString();

                    if (string.IsNullOrWhiteSpace(resultContent))
                    {
                        throw new NotSupportedException("Captcha solving failed.");
                    }

                    var resultObject = JsonSerializer.Deserialize<Dictionary<string, object?>>(resultContent);

                    if (resultObject == null)
                    {
                        throw new NotSupportedException("Captcha solving failed.");
                    }

                    var result = resultObject["text"]?.ToString();

                    return string.IsNullOrWhiteSpace(result)
                        ? throw new NotSupportedException(result)
                        : result;
                }
            }

            throw new NotSupportedException("Captcha solving failed.");
        }
        catch (Exception exception)
        {
            throw new NotSupportedException("Captcha solving failed.", exception);
        }
    }
}
