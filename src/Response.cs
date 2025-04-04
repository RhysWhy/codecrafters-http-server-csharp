namespace codecrafters_http_server.src;

public class Response
{
    public readonly string HttpVersion;
    public readonly int StatusCode;
    public readonly string StatusPhrase;

    public readonly IReadOnlyDictionary<string, string> Headers;

    public readonly string Body;

    public Response(string httpVersion, int statusCode, string statusPhrase, IReadOnlyDictionary<string, string> headers, string body)
    {
        HttpVersion = httpVersion;
        StatusCode = statusCode;
        StatusPhrase = statusPhrase;
        Headers = headers;
        Body = body;
    }

    public byte[] ToBytes()
    {
        var headers = string.Empty;
        foreach (var header in Headers)
        {
            headers += $"{header.Key}: {header.Value}\r\n";
        }
        return System.Text.Encoding.UTF8.GetBytes($"{HttpVersion} {StatusCode} {StatusPhrase}\r\n{headers}\r\n{Body}");
    }
}