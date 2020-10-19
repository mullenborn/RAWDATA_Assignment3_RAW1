using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        //[Required]
        public string Method { get; set; }
        //[Required]
        public string Path { get; set; }
        //[Required]
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
                
                if (!string.IsNullOrEmpty(msg))
                {
                    // Check for missing method error
                    if (msg == "{}")
                    {
                        var response = new Response
                        {
                            Body = "",
                            Status = "missing method"
                        };
                    
                        client.SendRequest(response.ToJson());
                        Console.WriteLine($"missing method");
                    }
                    
                    // Print message from client
                    Console.WriteLine($"Message from client {msg}");
                    
                    var requestFromClient = FromJson<Request>(msg);
                    var methods = new String[] { "create", "read", "update", "delete", "echo"};
                
                    // Check for illegal method
                    if (!methods.Contains(requestFromClient.Method))
                    {
                        var response = new Response
                        {
                            Body = "",
                            Status = "illegal method"
                        };
                        
                        client.SendRequest(response.ToJson());
                        Console.WriteLine("illegal method");
                    }
                    // test missing resource error - validates object
                    // var context = new ValidationContext(requestFromClient);
                    // var results = new List<ValidationResult>();
                    // var valid = Validator.TryValidateObject(requestFromClient, context, results);
                    // if (!valid)
                    // {
                    //     var response = new Response
                    //     {
                    //         Body = "",
                    //         Status = "missing resource"
                    //     };
                    //     
                    //     client.SendRequest(response.ToJson());
                    //     Console.WriteLine("missing resource");;
                    // }
                }
                // var data = Encoding.UTF8.GetBytes(msg.ToUpper());
                // stream.Write(data);
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