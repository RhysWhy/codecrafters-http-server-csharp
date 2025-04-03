using System.Net;
using System.Net.Sockets;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client

var requestBuffer = new byte[1024];
int receivedBytes = await socket.ReceiveAsync(requestBuffer);

var lines = System.Text.Encoding.UTF8.GetString(requestBuffer).Split("\r\n");

var line0Parts = lines[0].Split(" ");
var headers = lines.Length > 1 ? lines[1].Split("\r\n") : null;
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
    if(headers != null && headers.Any(h => h.StartsWith("User-Agent: ", StringComparison.OrdinalIgnoreCase)))
    {
        var userAgentHeader = headers.First(h => h.StartsWith("User-Agent: ", StringComparison.OrdinalIgnoreCase));
        content = userAgentHeader.Replace("User-Agent: ", string.Empty, StringComparison.OrdinalIgnoreCase)
                                 .Replace("\r\n", string.Empty);
    }
    response = $"{httpVerb} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
}

await socket.SendAsync(System.Text.Encoding.UTF8.GetBytes(response));