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
//test
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

                 CheckBadReqSystem(api, req, res, client);
                 
                
          //     client.SendRequest(res.ToJson());
                Console.WriteLine($"Message from client {msg}");
           
                var data = Encoding.UTF8.GetBytes(msg.ToUpper());
           
                stream.Write(data);
            }
        }

 

       public static void CheckBadReqSystem(Api api, Request req, Response res, TcpClient client)
        {
            string[] reasonResults = {"missing", "illegal"};
            string[] reqElements = {"method","path", "date", "body", "resource"};
            string[] methods = { "echo", "create", "read", "update", "delete"};
            string[] statusCodes = {"1 Ok", "2 Created", "3 Updated", "4 Bad Request", "5 Not Found", "6 Error"};
            string[] paths = { "api\\categories\\1", "\\api\\categories\\2", "\\api\\categories\\3", "testing" };
            bool hasMethod = true;
            bool hasPath = true;
            bool hasDate = true;
            bool hasBody = true;
            bool hasLegalMethod = false;
            bool hasLegalPath = false;
            bool hasLegalDate = false;
            bool hasLegalBody = false;
            
            string responseCode = "4";
            string responseResultMethod = "";
            string responseResultPath = "";
            string responseResultDate = "";
            string responseResultBody = "";
            string responseResultResource = "";
            string finalResultBadReq = "";
            string finalResultReq = "";
            
            // Checking for missing elements
            if (req.Method == null)
            {
                hasMethod = false;
                responseResultMethod = reasonResults[0] + " " + reqElements[0];
            }

            if (req.Path == null)
            {
                hasPath = false;
                responseResultPath = reasonResults[0] + " " + reqElements[1];
            }
            
            if (req.Date == null)
            {
                hasDate = false;
                responseResultDate = reasonResults[0] + " " + reqElements[2];
            }
            if (req.Body == null)
            {
                hasBody = false;
                responseResultBody = reasonResults[0] + " " + reqElements[3];
            }
            
            // missing resource
            if (hasMethod && hasDate && !hasBody && !hasPath)
            {
                responseResultResource = reasonResults[0] + " " + reqElements[4];
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
                    responseResultMethod = reasonResults[1] + " " + reqElements[0];
                }
                else
                {
                    hasLegalMethod = true;
                }

            }

            if (hasPath)
            {
                int pathCnt = 0;
                for (int i = 0; i < api.Paths.Count - 1; i++)
                {
                    if (req.Path == api.Paths[i].ToString())
                    {
                        pathCnt++;
                    }
                }

                if (pathCnt == 0)
                {
                    responseCode = statusCodes[4];
                    responseResultPath = reasonResults[1] + " " + reqElements[1];
                }
                else
                {
                    
                    hasLegalPath = true;
                }
            }

            if (hasDate)
            {
                if (req.Date.Length != 10)
                {
                    responseResultDate = reasonResults[1] + " " + reqElements[2];
                }
                else
                {
                    hasLegalDate = true;
                }
            }
            
            if (hasBody)
            {
                if (req.Body != null && req.Method != "echo")
                {
               
                    hasLegalBody = true;
                    //    responseResultBody = reasonResults[1] + " " + reqElements[3];
                } else if (req.Method == "echo" && !IsValidJSON(req.Body))
                {
                    
                    string responseResultEcho = req.Body;
                    res.Body = responseResultEcho;

                }
            
            }
            
            // READ
            if (hasLegalMethod && hasLegalPath && hasLegalDate)
            {

                responseCode = statusCodes[4];
                
                if (req.Path.Equals(api.Paths[0]))
                {
                    res.Body = api.Categories.ToJson();
                    responseCode = statusCodes[0];
                }
                else
                {
                    for (int i = 1; i < api.Categories.Count - 1; i++)
                    {
                        if (req.Path.Equals(api.Paths[i]))
                        {
                            responseCode = statusCodes[0];
                            res.Body = api.Categories[i - 1].ToJson();
                        }

                    }
                   
                }

            }





            if (hasLegalPath && hasLegalMethod && hasLegalDate)
            {
                
                finalResultReq = responseCode;
                res.Status = finalResultReq;
               // client.SendRequest(res.ToJson());
            }
            else
            {
                finalResultBadReq = responseCode + " " + responseResultMethod + " " + responseResultPath + " " + responseResultDate + " " + responseResultBody + " " + responseResultResource;
               
                res.Status = finalResultBadReq;
                Console.WriteLine(res.ToString());
               
            }

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