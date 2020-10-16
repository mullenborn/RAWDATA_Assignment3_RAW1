using System;
using System.Collections.Generic;
using System.Linq;
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

    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
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

                var request = FromJson<Request>(msg);
                
                var methods = new String[] { "create", "read", "update", "delete", "echo"};
                
                if (!methods.Contains(request.Method))
                {
                    var missingMethodResponse = new Response
                    {
                        Body = "",
                        Status = "illegal method"
                    };
                    
                    client.SendRequest(missingMethodResponse.ToJson());
                } 
                else if (msg == "{}")
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
        
        public static T FromJson<T>(this string element)
        {
            return JsonSerializer.Deserialize<T>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
        
        public static void SendRequest(this TcpClient client, string request)
        {
            var msg = Encoding.UTF8.GetBytes(request);
            client.GetStream().Write(msg, 0, msg.Length);
        }
    }
}