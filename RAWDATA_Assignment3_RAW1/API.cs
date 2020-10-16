namespace RAWDATA_Assignment3_RAW1
{
    public interface API<T>
    {

        void SetMsg(T msg);
        T GetMsg();
        T TrnsToJson(T msg);
        T TrnsFromJson(T msg);


    }
}