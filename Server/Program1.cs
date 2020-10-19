using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Collections;

namespace Server
{

    public class Api
    {
        public ArrayList Paths { get; set; }
        public ArrayList  Categories { get; set; }

    }


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
    
    
    public class Category
    {
        public int Cid { get; set; }
        public string Name { get; set; }
    }
    


  static class ServerProgram
    {
     
        
        static void Main(string[] args)
        {
    
            
            var server = new TcpListener(IPAddress.Loopback, 5000);
            server.Start();
            Console.WriteLine("Server started!");
            var api = new Api();
            
          ArrayList tempPaths = new ArrayList();
          tempPaths.Add("/api/categories");
          tempPaths.Add("/api/categories/1");
          tempPaths.Add("/api/categories/2");
          tempPaths.Add("/api/categories/3");
      
          ArrayList tempCategories = new ArrayList();
          tempCategories.Add(new Category {Cid = 1, Name = "Beverages"});
          tempCategories.Add(new Category {Cid = 2, Name = "Condiments"});
          tempCategories.Add(new Category {Cid = 3, Name = "Confections"});
          
            api.Categories = tempCategories;
            api.Paths = tempPaths;

            while (true)
            {
       
           
                var client = server.AcceptTcpClient();
                Console.WriteLine("Accepted client!");

                var stream = client.GetStream();

                var msg = Read(client, stream);
                
                var res = new Response
                {
                    Body = "",
                    Status = ""

                };

                 Request req =  msg.FromJson<Request>();
                 
                 Console.WriteLine(req.ToString());

                 CheckBadReqSystem(req, res, client);

                // CheckReqSystem(api, req, res, client);
                
          //     client.SendRequest(res.ToJson());
                Console.WriteLine($"Message from client {msg}");
           
                var data = Encoding.UTF8.GetBytes(msg.ToUpper());
           
                stream.Write(data);
            }
        }

 

       public static void CheckReqSystem(Api api, Request req, Response res, TcpClient client)
       {
           
           string[] responses = {"1 Ok", "2 Created", "3 Updated", "4 Bad Request", "5 Not Found", "6 Error"};
           string response = "";
           string body = "[";

           // FOR CREATE 

           if (req.Method == "create")
           {
               Console.WriteLine("CREATE!!");
               
           }

           // FOR READING
           
           if (req.Method == "read")
           {
               Console.WriteLine("READ!!");
               int cnt = 0;
               foreach (var p in api.Paths)
               {
                   if (req.Path.Contains(p.ToString()))
                   {

                       if (cnt >= 1)
                       {
                           response = responses[0];
                           body = api.Categories[cnt].ToJson();

                       }else if (cnt == 0)
                       {
                           response = responses[0];
                           int countTwo = 0;
                           foreach (var c in api.Categories)
                           {
                              
                               
                                   body += c.ToJson();
                                   if (countTwo < 2)
                                   {
                                       body += ",";
                                   }
                                   countTwo++;
                           }
                       }
                   }
                   cnt++;
               }
           } 

           if (req.Method == "update")
           {
               Console.WriteLine("UPDATE!!");
           }

           if (req.Method == "delete")
           {
               Console.WriteLine("DELETE!!!");
           }

           
           res.Body = body + "]";
           res.Status = response;
      
           
       }

     

       public static void CheckBadReqSystem(Request req, Response res, TcpClient client)
        {
            string[] reasonResults = {"missing", "illegal"};
            string[] elements = {"method","path", "date", "body", "resource"};
            string[] methods = { "echo", "create", "read", "update", "delete"};
            string[] paths = { "api\\categories\\1", "\\api\\categories\\2", "\\api\\categories\\3", "testing" };
            bool hasMethod = true;
            bool hasPath = true;
            bool hasDate = true;
            bool hasBody = true;
            string responseCode = "4";
            string responseResultMethod = "";
            string responseResultPath = "";
            string responseResultDate = "";
            string responseResultBody = "";
            string responseResultResource = "";
            string finalResult = "";
          
            
            // Checking for missing elements
            if (req.Method == null)
            {
                hasMethod = false;
                responseResultMethod = reasonResults[0] + " " + elements[0];
            }

            if (req.Path == null)
            {
                hasPath = false;
                responseResultPath = reasonResults[0] + " " + elements[1];
            }
            
            if (req.Date == null)
            {
                hasDate = false;
                responseResultDate = reasonResults[0] + " " + elements[2];
            }
            if (req.Body == null)
            {
                hasBody = false;
                responseResultBody = reasonResults[0] + " " + elements[3];
            }
            
            // missing resource
            if (hasMethod && hasDate && !hasBody && !hasPath)
            {
                responseResultResource = reasonResults[0] + " " + elements[4];
            }
            
            
            
            // checking Illegal
            if (hasMethod)
            {
                int methodCnt = 0;
                for (int i = 0; i < methods.Length - 1; i++)
                {
                    if (methods[i].Equals(req.Method))
                    {
                        methodCnt++;
                    }
                }

                if (methodCnt == 0)
                {
                    responseResultMethod = reasonResults[1] + " " + elements[0];
                }

            }

            if (hasPath)
            {
                int pathCnt = 0;
                for (int i = 0; i < paths.Length - 1; i++)
                {
                    if (req.Path == paths[i])
                    {
                        pathCnt++;
                    }
                }

                if (pathCnt == 0)
                {
                    responseResultPath = reasonResults[1] + " " + elements[1];
                }
            }

            if (hasDate)
            {
                if (req.Date.Length != 10)
                {
                    responseResultDate = reasonResults[1] + " " + elements[2];
                }
            }
            
            if (hasBody)
            {
                if (req.Body != null && req.Method != "echo")
                {
                    responseResultBody = reasonResults[1] + " " + elements[3];
                } else if (req.Method == "echo" && !IsValidJSON(req.Body))
                {
                    string responseResultEcho = req.Body;
                    res.Body = responseResultEcho;

                }
            }
            
            
                finalResult = responseCode + " " + responseResultMethod + " " + responseResultPath + " " + responseResultDate + " " + responseResultBody + " " + responseResultResource;
                res.Status = finalResult;
                Console.WriteLine(res.ToString());
                client.SendRequest(res.ToJson());

        }

        public static bool IsValidJSON(string input)
        {
            
            if (input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]"))
            {
                return true;
            }
            return false;
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
        public static Response ReadResponse(this TcpClient client)
        {
            var strm = client.GetStream();
            //strm.ReadTimeout = 250;
            byte[] resp = new byte[2048];
            using (var memStream = new MemoryStream())
            {
                int bytesread = 0;
                do
                {
                    bytesread = strm.Read(resp, 0, resp.Length);
                    memStream.Write(resp, 0, bytesread);

                } while (bytesread == 2048);
                
                var responseData = Encoding.UTF8.GetString(memStream.ToArray());
                return JsonSerializer.Deserialize<Response>(responseData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            }
        }

    


    }
}