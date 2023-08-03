using System.Net.Http;

using HtmlAgilityPack;

using Microsoft.Extensions.Logging;

using Telegram.ApAzureBot.Services.Interfaces;

namespace Telegram.ApAzureBot.Services.Implementations;

public sealed class WebService : IWebService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly string _serbianMidRfSecretQueryParameters;

    public WebService(ILogger<WebService> logger, IHttpClientFactory httpClientFactory)
    {
        var serbianMidRfSecretQueryParameters = Environment.GetEnvironmentVariable("SerbianMidRfSecretQueryParameters", EnvironmentVariableTarget.Process);

        ArgumentNullException.ThrowIfNull(serbianMidRfSecretQueryParameters);

        _serbianMidRfSecretQueryParameters = serbianMidRfSecretQueryParameters;

        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CheckSerbianMidRf()
    {
        var client = _httpClientFactory.CreateClient(Constants.MidRfHttpClientName);

        var page = await client.GetStringAsync(client.BaseAddress + _serbianMidRfSecretQueryParameters);

        var htmlDocument = new HtmlDocument();

        htmlDocument.LoadHtml(Page);

        var captchaQueryParameter = htmlDocument.DocumentNode
            .SelectNodes("//img")
            .Where(x => x is not null)
            .Select(x => x.GetAttributeValue("src", ""))
            .FirstOrDefault(x => x.Contains("CodeImage", StringComparison.OrdinalIgnoreCase));

        ArgumentNullException.ThrowIfNull(captchaQueryParameter);

        await Task.Delay(10000);

        var captcha = await client.GetByteArrayAsync(client.BaseAddress + captchaQueryParameter);

        var captchaResult = "123456";

        //File.WriteAllBytes(Environment.CurrentDirectory + "/captcha.jpeg", captcha);
        
        //First Post,200

        //Second Post -> 302 - нет мест


        return "The function is not implemented yet.";
    }

    private const string Page = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head><title>\r\n\tЗапись на прием\r\n</title><link rel=\"icon\" href=\"/favicon.ico\" type=\"image/x-icon\"><link rel=\"shortcut icon\" href=\"/favicon.ico\" type=\"image/x-icon\"><link rel=\"stylesheet\" type=\"text/css\" href=\"css/Styles.css?v=01\"><link rel=\"stylesheet\" type=\"text/css\" href=\"css/StyleSheet.css?v=01\">\r\n    <style type=\"text/css\">\r\n    #wait_answer \r\n    {\r\n    \tposition:fixed;\r\n    \ttop: 50%;\r\n    \tleft: 50%;\r\n    \tvisibility:hidden;\r\n    \tdisplay:block;\r\n    \theight:65px;\r\n    \twidth:65px;\r\n    \tbackground-image: url(_images/GEARS_AN.GIF); background-repeat: no-repeat;\r\n    \tz-index:100;\r\n    }\r\n    </style>\r\n    <script type=\"text/javascript\">\r\n    <!--\r\n        function onClickButton() {\r\n        var item = document.getElementById(\"wait_answer\");\r\n        item.style.visibility = \"visible\";\r\n    }\r\n    //-->\r\n    </script>\r\n\r\n</head>\r\n<body>\r\n<div id=\"body\">\r\n    <div id=\"top\"> \r\n\t    <span>\r\n\t\t    \r\n\r\n\t    </span>\r\n    </div>\r\n\r\n<div id=\"hdr\"> \r\n\t    <div id=\"hdr2\">\r\n\t        <div class=\"gerb\"></div>\r\n            <span class=\"header\">\r\n                <div class=\"text\">Консульский отдел Посольства<br>Российской Федерации<br> в Сербии (Белград) <br><br>\r\n                <span style=\"font-size:15px; color:#fff;\">Система электронной записи на прием</span>\r\n                </div> \r\n    \t\t</span>             \r\n               \r\n        </div>\r\n\t</div>\r\n\t\r\n\t\r\n<div id=\"desk\">\r\n            \r\n<form name=\"aspnetForm\" method=\"post\" action=\"OrderInfo.aspx?id=54437&amp;cd=9CFA9945\" id=\"aspnetForm\" onsubmit=\"WaitAnswer()\">\r\n<input type=\"hidden\" name=\"__VIEWSTATE\" id=\"__VIEWSTATE\" value=\"/wEPDwUKLTc5Nzk3MjA1NQ9kFgJmD2QWAgIFD2QWAgIBDxYCHghvbnN1Ym1pdAUMV2FpdEFuc3dlcigpFgICAw9kFgQCBg8PFgIeDEVycm9yTWVzc2FnZQUMRXJyb3JNZXNzYWdlZGQCEQ8PFgIeCEltYWdlVXJsBRh+L0NvZGVJbWFnZS5hc3B4P2lkPWMzMDVkZGQsq0aoQMs0a2UiNUKdPfVQuibZeg==\">\r\n\r\n<input type=\"hidden\" name=\"__EVENTVALIDATION\" id=\"__EVENTVALIDATION\" value=\"/wEWBQLax4agCQLmjdfGDQKLs7ufCwK5ysLjCwKj8MqYCG6wdbkggtFHM8sLORAA36tzcBtR\">\r\n\r\n<table><tr><td id=\"left-panel\">\r\n        <div class=\"box_instruction\" style=\"display:none;\"><p>Заполните поля информацией, полученной при оформлении записи, из распечатки «Подтверждения записи на прием», нажмите кнопку «Далее».</p></div><div class=\"box_instruction\" style=\"display:none;\"><p>Для проверки наличия свободного времени для записи на прием нажмите на кнопку «Записаться на прием»</p></div>\r\n</td>\r\n<td id=\"center-panel\">\r\n \r\n \r\n <script>\r\n    document.getElementsByClassName('box_instruction')[0].style.display = '';\r\n    document.getElementsByClassName('box_instruction')[1].style.display = 'none';\r\n</script>\r\n\r\n    <h1>\r\n        <span id=\"ctl00_MainContent_Label_Header\"><h1>ИНФОРМАЦИЯ О ЗАЯВКЕ</h1></span>\r\n    </h1>\r\n    <br>\r\n    <p>\r\n    Номер заявки\r\n    </p>\r\n    <div class=\"inp\" style=\"margin-left:20px;\">\r\n    <input name=\"ctl00$MainContent$txtID\" type=\"text\" value=\"54437\" id=\"ctl00_MainContent_txtID\">\r\n    </div>\r\n    <p> \r\n    Защитный код\r\n    </p>\r\n    <div class=\"inp\" style=\"margin-left:20px;\">\r\n    <input name=\"ctl00$MainContent$txtUniqueID\" type=\"text\" value=\"9CFA9945\" id=\"ctl00_MainContent_txtUniqueID\">\r\n    <p>\r\n    \r\n    </p>\r\n    </div>\r\n    <br>\r\n\r\n    <div style=\"margin-bottom:10px;margin-top: 5px; font-size:13px; font-weight:bold;\">\r\n    Введите символы с картинки.\r\n    </div>\r\n    <div class=\"inp\" style=\"margin-top:10px;\">\r\n    <img id=\"ctl00_MainContent_imgSecNum\" src=\"CodeImage.aspx?id=c305\" alt=\"Необходимо включить загрузку картинок в браузере.\" border=\"0\" style=\"width:140px;\"><input name=\"ctl00$MainContent$txtCode\" type=\"text\" id=\"ctl00_MainContent_txtCode\">\r\n    </div>\r\n    <div>\r\n    \r\n    </div>\r\n <input type=\"submit\" name=\"ctl00$MainContent$ButtonA\" value=\"Далее\" onclick=\"javascript:WebForm_DoPostBackWithOptions(new WebForm_PostBackOptions(&quot;ctl00$MainContent$ButtonA&quot;, &quot;&quot;, true, &quot;&quot;, &quot;&quot;, false, false))\" id=\"ctl00_MainContent_ButtonA\" class=\"btn\">\r\n \r\n \r\n \r\n    <div>\r\n      \r\n    </div>\r\n</td></tr></table>\r\n</form>\r\n<div id=\"wait_answer\">\r\n<p> \r\n</p></div>\r\n<script>\r\n\r\nif(document.getElementsByClassName('inp').length==0)\r\n{\r\ndocument.getElementsByClassName('box_instruction')[0].style.display='none';\r\n}\r\n</script>\r\n\r\n       \r\n    </div>\r\n    \r\n<div style=\"clear:both;\"></div> \r\n   \r\n<div id=\"footer\">\r\n    <div class=\"rightfooter\">\r\n        <p>© Сайт КД МИД России \"Запись на прием в КЗУ\" <br>2014 - 2018</p>\r\n     </div>\r\n    <div style=\"clear:both;\"></div>\r\n</div>    \r\n</div>    \r\n</body>\r\n</html>";

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
