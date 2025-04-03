using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client

var requestBuffer = new byte[1024];
int receivedBytes = await socket.ReceiveAsync(requestBuffer);

var lines = System.Text.Encoding.UTF8.GetString(requestBuffer).Split("\r\n");

var line0Parts = lines[0].Split(" ");
var (method, pathParts, httpVerb) = (line0Parts[0], line0Parts[1].Split("/", StringSplitOptions.RemoveEmptyEntries), line0Parts[2]);

string response;

switch (pathParts[0])
{
    case "/": { response = $"{httpVerb} 200 OK\r\n\r\n"; break; }
    case "echo":
        {
            var content = pathParts[1];
            response = $"{httpVerb} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
            break;
        }
    default: { response = $"{httpVerb} 404 Not Found\r\n\r\n"; break; }
}

await socket.SendAsync(System.Text.Encoding.UTF8.GetBytes(response));