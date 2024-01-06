using System.Globalization;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using Net.Shared.Bots.Abstractions.Interfaces;

namespace KdmidScheduler.Azure.Api;

public class Functions(IBotClient client)
{
    private readonly IBotClient _client = client;

    private const string ReceiveFunction = "receive";
    private const string ListenFunction = "listen";

    [Function(ListenFunction)]
    public Task SetReceiver([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request, CancellationToken cToken)
    {
        var url = request.Url.ToString().Replace(ListenFunction, ReceiveFunction, true, CultureInfo.InvariantCulture);

        var uri = new Uri(url);

        return _client.Listen(uri, cToken);
    }

    [Function(ReceiveFunction)]
    public async Task Receive([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request, CancellationToken cToken)
    {
        var data = await request.ReadAsStringAsync();

        ArgumentNullException.ThrowIfNull(data, "Received data is empty.");

        await _client.Receive(data, cToken);
    }
}
