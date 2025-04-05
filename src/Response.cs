using System.Text;

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
        var gzip = false;
        foreach (var header in Headers)
        {
            if (header.Key == "Content-Encoding" && header.Value.Contains("gzip")) { gzip = true; }
            headers += $"{header.Key}: {header.Value}\r\n";
        }

        if (gzip)
        {
            return [..Encoding.UTF8.GetBytes($"{HttpVersion} {StatusCode} {StatusPhrase}\r\n{headers}\r\n"), ..Encoding.UTF8.GetBytes(Body)];
        }
        else
        {
            return Encoding.UTF8.GetBytes($"{HttpVersion} {StatusCode} {StatusPhrase}\r\n{headers}\r\n{Body}");
        }
    }
}