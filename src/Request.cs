namespace codecrafters_http_server.src;

public class Request
{
    public readonly string HttpMethod;
    public readonly string Path;
    public readonly string HttpVersion;

    public readonly IReadOnlyDictionary<string, string> Headers;

    public readonly string Body;

    private Request(string httpMethod, string path, string httpVersion, IReadOnlyDictionary<string, string> headers, string body)
    {
        HttpMethod = httpMethod;
        Path = path;
        HttpVersion = httpVersion;
        Headers = headers;
        Body = body;
    }

    public static Request Parse(string inputString)
    {
        var httpMethod = string.Empty;
        var path = string.Empty;
        var httpVersion = string.Empty;
        var headers = new Dictionary<string, string>();
        var body = string.Empty;

        try
        {
            // Split on \r\n\r\n to divide into two segments
            // 1: Request Line and Headers
            // 2: Body
            var segments = inputString.Split("\r\n\r\n");

            // Split the first segment into request line (first entry) and headers (every other entry) separately
            var requestAndHeaders = segments[0].Split("\r\n");

            var requestParts = requestAndHeaders[0].Split(" ");
            (httpMethod, path, httpVersion) = (requestParts[0], requestParts[1], requestParts[2]);

            for (int i = 1; i < requestAndHeaders.Length; i++)
            {
                var headerParts = requestAndHeaders[i].Split(": ", StringSplitOptions.RemoveEmptyEntries);
                if (headerParts.Length >= 2)
                {
                    var headerName = headerParts[0];
                    var headerValue = headerParts[1];

                    headers.TryAdd(headerName, headerValue);
                }
            }

            body = segments[1];

            return new Request(httpMethod, path, httpVersion, headers, body);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public override string ToString()
    {
        return $"{HttpMethod} {Path} {HttpVersion} {Headers} {Body}";
    }
}