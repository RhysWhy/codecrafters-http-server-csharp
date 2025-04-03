using System.Net;
using System.Net.Sockets;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true)
{
    var socket = server.AcceptSocket(); // wait for client
    Task.Run(() => HandleSocket(socket));
}

static Task HandleSocket(Socket socket)
{
    var requestBuffer = new byte[1024];
    int receivedBytes = socket.Receive(requestBuffer);

    var lines = System.Text.Encoding.UTF8.GetString(requestBuffer).Split("\r\n");

    var line0Parts = lines[0].Split(" ");
    var (method, pathParts, httpVerb) = (line0Parts[0], line0Parts[1].Split("/", StringSplitOptions.RemoveEmptyEntries), line0Parts[2]);

    var response = $"{httpVerb} 404 Not Found\r\n\r\n";

    if (pathParts == null || pathParts.Length == 0) { response = $"{httpVerb} 200 OK\r\n\r\n"; }
    else if (pathParts[0] == "echo")
    {
        var content = pathParts[1];
        response = $"{httpVerb} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
    }
    else if (pathParts[0] == "user-agent")
    {
        var content = string.Empty;
        if (lines.Any(h => h.StartsWith("User-Agent: ", StringComparison.OrdinalIgnoreCase)))
        {
            var userAgentHeader = lines.First(h => h.StartsWith("User-Agent: ", StringComparison.OrdinalIgnoreCase));
            content = userAgentHeader.Replace("User-Agent: ", string.Empty, StringComparison.OrdinalIgnoreCase)
                                     .Replace("\r\n", string.Empty);
        }
        response = $"{httpVerb} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
    }
    else if (pathParts[0] == "files")
    {
        var arguments = Environment.GetCommandLineArgs();
        var directory = arguments[2];
        var fileName = pathParts[1];
        var fileLocation = Path.Combine(directory, fileName);
        if (method == "GET")
        {
            if (File.Exists(fileLocation))
            {
                var content = File.ReadAllText(fileLocation);
                response = $"{httpVerb} 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {content.Length}\r\n\r\n{content}";
            }
        }
        else if (method == "POST")
        {
            if (!File.Exists(fileLocation))
            {
                var requestBody = pathParts[pathParts.Length - 1];
                File.WriteAllText(fileLocation, requestBody);
                response = $"{httpVerb} 201 Created\r\n\r\n";
            }
        }
    }

    socket.Send(System.Text.Encoding.UTF8.GetBytes(response));

    return Task.CompletedTask;
}