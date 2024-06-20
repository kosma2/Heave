using Microsoft.Data.SqlClient;

namespace Heave
{
    partial class Program
    {
    public static SqlConnection GetConnection(String SqlStr)
    {
        try
        {
            System.Console.WriteLine("Connecting to database...");
            SqlConnection connection = new(SqlStr);
            return connection;
        }
        catch (SqlException e)
        {
            Console.WriteLine(e.ToString());
            System.Console.WriteLine("connection returned nyull");
            return null;
        }
    }
    public class UserSession
    {
        public int MemberId {get; private set;}
        public int CustomerId {get; private set;}
        public UserSession(int memberId, int custId)
        {
            MemberId = memberId;
            CustomerId = custId;
        }
    }
}
}