using codecrafters_http_server.src;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    var socket = server.AcceptSocket(); // wait for client
    Task.Run(() => HandleSocket(socket));
}

static Task HandleSocket(Socket socket)
{
    var arguments = Environment.GetCommandLineArgs();
    var directory = string.Empty;
    if (arguments.Length >= 3) { directory = arguments[2]; }

    var requestBuffer = new byte[1024];
    int receivedBytes = socket.Receive(requestBuffer);

    var requestString = Encoding.UTF8.GetString(requestBuffer);

    var request = Request.Parse(requestString);
    var response = HandleRequest(directory, request);

    socket.Send(Encoding.UTF8.GetBytes(response.ToString()));

    return Task.CompletedTask;
}

static Response HandleRequest(string directory, Request request)
{
    var statusCode = 404;
    var statusPhrase = "Not Found";
    var headers = new Dictionary<string, string>();
    var body = string.Empty;

    var gzip = false;
    if (request.Headers.ContainsKey("Accept-Encoding"))
    {
        var encodings = request.Headers["Accept-Encoding"].Split(",", StringSplitOptions.TrimEntries);
        if (encodings.Contains("gzip"))
        {
            headers.Add("Content-Encoding", "gzip");
            gzip = true;
        }
    }

    if (request.Path == "/") { statusCode = 200; statusPhrase = "OK"; }
    else if (request.Path.StartsWith("/echo/"))
    {
        body = request.Path["/echo/".Length..];
        if (gzip)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(body);
            using var memStream = new MemoryStream();
            using var gzipStream = new GZipStream(memStream, CompressionMode.Compress, true);
            gzipStream.Write(bodyBytes, 0, bodyBytes.Length);
            gzipStream.Flush();
            gzipStream.Close();

            body = Encoding.UTF8.GetString(memStream.ToArray());
        }

        headers.Add("Content-Type", "text/plain");
        headers.Add("Content-Length", body.Length.ToString());
        statusCode = 200;
        statusPhrase = "OK";
    }
    else if (request.Path.StartsWith("/user-agent"))
    {
        var hasHeader = request.Headers.TryGetValue("User-Agent", out string userAgentHeader);
        if (hasHeader)
        {
            body = userAgentHeader;
            headers.Add("Content-Type", "text/plain");
            headers.Add("Content-Length", body.Length.ToString());
            statusCode = 200;
            statusPhrase = "OK";
        }
    }
    else if (request.Path.StartsWith("/files/"))
    {
        var fileName = request.Path["/files/".Length..];
        var fileLocation = Path.Combine(directory, fileName);
        if (request.HttpMethod == "GET")
        {
            if (File.Exists(fileLocation))
            {
                body = File.ReadAllText(fileLocation);
                headers.Add("Content-Type", "application/octet-stream");
                headers.Add("Content-Length", body.Length.ToString());
                statusCode = 200;
                statusPhrase = "OK";
            }
        }
        else if (request.HttpMethod == "POST")
        {
            var hasHeader = request.Headers.TryGetValue("Content-Length", out string contentLengthHeader);
            if (hasHeader)
            {
                var contentLength = int.Parse(contentLengthHeader);
                File.WriteAllText(fileLocation, request.Body.AsSpan(0, contentLength));

                statusCode = 201;
                statusPhrase = "Created";
            }
        }
    }

    return new Response(request.HttpVersion, statusCode, statusPhrase, headers, body);
}