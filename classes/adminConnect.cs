using System.Text;
using Microsoft.Data.SqlClient;

namespace Heave
{
    partial class Program
    {
        public class adminConnect : DbConnection
        {
            public UserSession CurrentSession { get; private set; }

            public override List<(int, string)> ShowCustomers()
            {
                List<(int, string)> custInfo = new();
                using (SqlConnection connection = GetConnection(SqlStr))
                {
                    String sql = "SELECT CustomerId, LastName FROM customer";
                    using (SqlCommand command = new(sql, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int custId = reader.GetInt32(reader.GetOrdinal("CustomerId"));
                                string custName = reader.GetString(reader.GetOrdinal("LastName"));
                                custInfo.Add((custId, custName));
                            }
                            return custInfo;
                        }
                    }
                }
            }
            public List<(int memberId, String memberName, int memRole)> DBListMembers() // displays members in tuples (memId, login, role-0for mem, 1 for admin, 2 for customer)
            {
                SqlConnection connecti = GetConnection(SqlStr);
                using (connecti)
                {
                    String sql = "SELECT MemberId, Login, Role FROM Member;";
                    using (SqlCommand command = new SqlCommand(sql, connecti))
                    {
                        connecti.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int rowCount = 0;
                                List<(int, string, int)> memInfo = new();
                                while (reader.Read())
                                {
                                    int memRole = 1;
                                    int memId = Convert.ToInt32(reader["MemberId"]);
                                    string memLogin = reader["Login"].ToString();
                                    int? memRol = reader["Role"] != DBNull.Value ? Convert.ToInt32(reader["Role"]) : (int?)null;
                                    if (memRol == null)
                                    {
                                        memRole = 0;
                                    }

                                    memInfo.Add((memId, memLogin, memRole));
                                }
                                return memInfo;
                            }
                            else
                            {
                                Console.WriteLine("No rows found.");
                                reader.Close();
                                return null;
                            }
                        }
                    }
                }
            }
            public override string GetItemName(int itemId)
            {
                using (SqlConnection connection = GetConnection(SqlStr))
                {
                    String sql = "SELECT ItemName FROM inventory WHERE ItemId = @itemId";
                    using (SqlCommand command = new(sql, connection))
                    {
                        command.Parameters.AddWithValue("@itemId", itemId);
                        connection.Open();
                        string resultItemName = Convert.ToString(command.ExecuteScalar());
                        return resultItemName;
                    }
                }
            }
            public override int DBGetCustomerId(int memId)// retrieves MemberId from customer table
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    String sqlCommand = "SELECT CustomerId FROM customer WHERE CustomerId = @memId";
                    using (SqlCommand command = new(sqlCommand, connection))
                    {
                        command.Parameters.AddWithValue("@memId", memId);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int resultMemId = reader.GetInt32(0);
                                return resultMemId;
                            }
                            else
                            {
                                System.Console.WriteLine("nothing to read here");
                                return -1;
                            }
                        }
                    }
                }
            }
            public override int DBdeleteOrder(int orderId)  // deletes the order and associated orderItems
            {
                using (SqlConnection connection = GetConnection(SqlStr))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        // First, delete from the 'orderItems' table
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "DELETE FROM orderItems WHERE OrderId = @orderId";
                            command.Parameters.AddWithValue("@orderId", orderId);
                            command.ExecuteNonQuery();
                            System.Console.WriteLine($"deleting order items");
                        }

                        // Then delete from the 'orders' table
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "DELETE FROM orders WHERE OrderId = @orderId";
                            command.Parameters.AddWithValue("@orderId", orderId);
                            command.ExecuteNonQuery();
                            System.Console.WriteLine($"deleting order");

                        }

                        // Commit the transaction
                        transaction.Commit();

                        // Return the deleted OrderId
                        return orderId;
                    }
                    catch (Exception)
                    {
                        if (transaction != null)
                        {
                            transaction.Rollback();
                        }
                        throw; // Re-throw the exception for handling
                    }
                }
            }
            public override (int, int) InterfaceCreateOrder()
            {
                return (0, 0);
            }
            public override int DBCreateOrder(int custId, int itemId, int quantity)
            {
                return 0;

            }

            public override List<(int ItemId, String ItemName)> DBListItems() // dispays all items in inventory [itemId][itemName]
            {
                List<(int, string)> idAndName = new();

                return idAndName;
            }
            public override int DBCheckLogin(Member mem)// creates a usersession, returns member id if logged in, "-1" for password mismatch, "-2" for no such user
            {
                SqlConnection connect = GetConnection(SqlStr);
                using (connect)
                {
                    // GETTING PASSWORD HASH
                    String sql = "SELECT PasswordHash, Salt, MemberId FROM member WHERE Login = @login";
                    using (SqlCommand command = new(sql, connect))
                    {
                        connect.Open();
                        command.Parameters.AddWithValue("@login", mem.login);
                        using SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            string resultHash = reader.GetString(0);
                            byte[] resultSalt = new byte[32];
                            int resultMemId = reader.GetInt32(2);
                            reader.GetBytes(reader.GetOrdinal("Salt"), 0, resultSalt, 0, 32);
                            string inputHash = SecurityHelper.HashPassword(mem.pass, resultSalt);
                            if (inputHash == resultHash)
                            {
                                System.Console.WriteLine("user logged in");
                                System.Console.WriteLine("MemberId is " + resultMemId);
                                int custId = this.DBGetCustomerId(resultMemId);  // retrieve customer id from DB
                                CurrentSession = new(resultMemId, custId);  // create in-class user session
                                return resultMemId;
                            }
                            else
                            {
                                System.Console.WriteLine("login failed");
                                return -1;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No user found with username: {mem.login}");
                            return -2;
                        }
                    }
                }

            }
            public override bool DBCreateCustomer(Customer cust)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    //IMPORTANT: To construct the SQL command securely while including the execution of the geometry::STPointFromText function directly in the command text (due to the nature of spatial data functions), non spatial data is parameterized while the spatial part is insterted dynamcally.
                    //SQL STRING BUILD
                    StringBuilder sb = new();
                    sb.Append("USE master; ");
                    sb.Append("INSERT INTO customer (FirstName, LastName, HomeAddress, GeoPoint, DateCreated) VALUES ");
                    sb.Append("(@fName, @lName, @hAddress, geography::STPointFromText('POINT('+ @Coords +')', 4326), @date);");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@fName", cust.FirstName);
                        command.Parameters.AddWithValue("@lName", cust.LastName);
                        command.Parameters.AddWithValue("@hAddress", cust.HomeAddress);
                        command.Parameters.AddWithValue("@Coords", cust.Coordinates);//format (long lat) eg.  -117.29658128341528 49.477015693801576 
                        command.Parameters.AddWithValue("@date", cust.DateCreated);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + " row(s) inserted");
                        return rowsAffected > 0;
                    }
                }
            }
            public override int DBCreateMember(Member memb)//REDUNDANT FIX RETURN
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    //SQL STRING BUILD
                    StringBuilder sb = new();
                    sb.Append("USE master; ");
                    sb.Append("INSERT INTO member (Login, PasswordHash, Salt) VALUES ");
                    sb.Append("(@login, @passHash, @salt);");
                    String sql = sb.ToString();

                    // HASH AND SALT
                    Byte[] salt = SecurityHelper.CreateSalt();
                    String passHash = SecurityHelper.HashPassword(memb.pass, salt);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", memb.login);
                        command.Parameters.AddWithValue("@passHash", passHash);
                        command.Parameters.AddWithValue("@salt", salt);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + " row(s) inserted");
                    }
                }
                return -5;
            }
            public override void DBAddItem(Item item)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    StringBuilder sb = new();
                    sb.Append("USE master; ");
                    sb.Append("INSERT INTO inventory (ItemId, ItemName, ItemDescr, ItemPrice, ItemDiment, ItemWeight) VALUES ");
                    sb.Append("(@id, @itName, @itDescr, @itPrice, @itDimen, @itWeight);");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", item.itemId);
                        command.Parameters.AddWithValue("@itName", item.itemName);
                        command.Parameters.AddWithValue("@itDescr", item.itemDesc);
                        command.Parameters.AddWithValue("@itPrice", item.itemPrice);
                        command.Parameters.AddWithValue("@itDimen", item.itemDimens);
                        command.Parameters.AddWithValue("@itWeight", item.itemWeight);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + " row(s) inserted");
                    }
                }
            }
            public override void DBDeleteItem(string id)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    StringBuilder sb = new();
                    sb.Append("DELETE FROM inventory WHERE id = @id");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + " row(s) inserted");
                    }
                }
            }
            public void DBUpdateCustomer(String id)//needs change
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    String userToUpdate = "juju";
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE Customers SET Location = N'Some Place St' WHERE LastName = @lastName");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@lastName", userToUpdate);
                        int rowsAffected = command.ExecuteNonQuery();
                        System.Console.WriteLine(rowsAffected);
                    }
                }
            }
            public bool DBUpdateMemberRole(int memberId, int role) //true if rowsAffected
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("UPDATE Member SET Role = @role WHERE MemberId = @id;");
                    String sql = sb.ToString();
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", memberId);
                        command.Parameters.AddWithValue("@role", role);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            public void DBDeleteCustomer(int CustomerId)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    StringBuilder sb = new();
                    sb.Append("DELETE FROM Customer WHERE CustomerId = @custId");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@custId", CustomerId);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        System.Console.WriteLine(rowsAffected);
                    }
                }
            }
            public void DBDeleteMember(int memberId)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    StringBuilder sb = new();
                    sb.Append("DELETE FROM Member WHERE memberId = @memId");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@memId", memberId);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        System.Console.WriteLine(rowsAffected);
                    }
                }
            }
            public override decimal GetItemPrice(int itemId)
            { return 0; }
            public override int DBcreateOrderItem(int orderId, int itemId, int quantity, SqlConnection connection, SqlTransaction transaction)
            {
                return 0;
            }
        }
    }
}
