namespace RAWDATA_Assignment3_RAW1
{
    public class Category
    {
        private int cid;
        private string name;

        public Category(int id, string _name)
        {
            
            cid = id;
            name = _name;

        }

        public string GetName(){return name;}
        public int GetId(){return cid;}


    }
}