using System.Text;
using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.Http;

/// <summary>
/// Register under <see cref="HttpActivityTypes.Request"/>. Params: Url (required), Method (optional, default GET),
/// Body (optional), ContentType (optional, default application/json, only used when Body is set), Headers
/// (optional, one "Key: Value" per line). Any non-2xx response throws — recorded as the activity's failure — with
/// the status and response body in the message. On success the response body becomes the ActivityExecution's Output.
/// </summary>
public sealed class HttpRequestActivityHandler(HttpClient httpClient) : IActivityHandler
{
    public async Task<string?> ExecuteAsync(ActivityExecutionContext context, CancellationToken cancellationToken)
    {
        var url = context.GetRequired("Url");
        var method = context.GetOptional("Method", "GET")!;
        var body = context.GetOptional("Body");
        var contentType = context.GetOptional("ContentType", "application/json")!;

        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        if (body is not null)
            request.Content = new StringContent(body, Encoding.UTF8, contentType);

        foreach (var (key, value) in ParseHeaders(context.GetOptional("Headers")))
            request.Headers.TryAddWithoutValidation(key, value);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase} calling {method} {url}: {responseBody}",
                inner: null,
                response.StatusCode);

        return responseBody;
    }

    private static IEnumerable<(string Key, string Value)> ParseHeaders(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            yield break;

        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = line.IndexOf(':');
            if (separatorIndex <= 0)
                continue;

            yield return (line[..separatorIndex].Trim(), line[(separatorIndex + 1)..].Trim());
        }
    }
}
