using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Server
{
    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }
    }
    static class ServerProgram
    {
        static void Main(string[] args)
        {
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine("Server started!");

            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine("Accepted client!");

                var stream = client.GetStream();
                
                var msg = Read(client, stream);

                if (msg == "{}")
                {

                    var missingMethodResponse = new Response
                    {
                        Body = "",
                        Status = "missing method"
                    };
                    
                    client.SendRequest(missingMethodResponse.ToJson());
                }
                else
                {
                    Console.WriteLine($"Message from client {msg}");

                    var data = Encoding.UTF8.GetBytes(msg.ToUpper());

                    stream.Write(data);
                }
            }
        }

        private static string Read(TcpClient client, NetworkStream stream)
        {
            byte[] data = new byte[client.ReceiveBufferSize];

            var cnt = stream.Read(data);

            var msg = Encoding.UTF8.GetString(data, 0, cnt);
            return msg;
        }
        
        public static string ToJson(this object data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        
        public static void SendRequest(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }
    }
}