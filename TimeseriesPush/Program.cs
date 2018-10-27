using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimeseriesPush
{
    class Program
    {
        public static void Main(string[] args)
        {
            Task t = Echo();
            t.Wait();
        }

        private static async Task Echo()
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Console.Write("Input wss host: ");
                string host = Console.ReadLine();
                Console.Write("Input bearer token: ");
                string token = Console.ReadLine();
                Console.Write("Input zone id: ");
                string zoneId = Console.ReadLine();
                Console.Write("Input origin: ");
                string origin = Console.ReadLine();
                ws.Options.SetRequestHeader("Authorization", "Bearer " + token);
                ws.Options.SetRequestHeader("Predix-Zone-Id", zoneId);
                ws.Options.SetRequestHeader("Origin", origin);
                Uri serverUri = new Uri(host);
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                while (ws.State == WebSocketState.Open)
                {
                    Console.Write("Input filepath or ('exit' to exit): ");
                    string msg = Console.ReadLine();
                    if (msg == "exit")
                    {
                        break;
                    }
                    string contents = File.ReadAllText(msg);
                    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(contents));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);
                    ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, CancellationToken.None);
                    Console.WriteLine(Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count));
                }
            }
        }
    }
}
