using System;
using System.Text.Json;

namespace RAWDATA_Assignment3_RAW1
{
    class Program : API<Category>
    {

   // Category[] categories = new Category[7];

   // private int[] statusCodes = {1,2,3,4,5,6};
   private int statusCode;
   private string[] phrases = {"Ok", "Created", "Updated", "Bad Request", "Not Found", "Error"};
        
        
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //test nils
            
            

            


        }


        public void SetMsg(Category msg)
        {
            throw new NotImplementedException();
        }

        public Category GetMsg()
        {
            throw new NotImplementedException();
        }

        public Category TrnsToJson(Category msg)
        {
            throw new NotImplementedException();
        }

        public Category TrnsFromJson(Category msg)
        {
            throw new NotImplementedException();
        }

        public void Create()
        {
            
            
            
        }
    }
}