using System.Data;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.Data.SqlClient;


namespace Heave
{
    partial class Program
    {
        public class userConnect : DbConnection
        {
            public UserSession CurrentSession { get; private set; }
            public override List<(int, string)> ShowCustomers()
            {
                return null;
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
                        }

                        // Then delete from the 'orders' table
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "DELETE FROM orders WHERE OrderId = @orderId";
                            command.Parameters.AddWithValue("@orderId", orderId);
                            command.ExecuteNonQuery();
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
            public void ShowOrdersNitems(int customerId)
            {
                List<List<String>> orders = DBListOrders(customerId);
                foreach (List<string> Item in orders)
                {
                    int orderId = Convert.ToInt32(Item[0]);
                    System.Console.WriteLine("Items for order id " + orderId);
                    List<string> items = DBListOrderItems(orderId);
                    foreach (string tems in items)
                    {
                        System.Console.WriteLine("Item Id is " + tems);
                    }
                }
            }
            public override (int, int) InterfaceCreateOrder()  // interface for creating order, returns a tuple (item#, quantity)
            {
                userConnect usConnect = new userConnect();

                System.Console.WriteLine("please order #");
                int itemNo = Convert.ToInt32(Console.ReadLine());
                System.Console.WriteLine("how many you want?");
                int quant = Convert.ToInt32(Console.ReadLine());
                return (itemNo, quant);//(inputItemId, quant);

            }
            public override int DBCreateOrder(int custId, int itemId, int quantity) //returns orderId
            {
                String custAddress = GetCustomerAddress(custId);
                System.Console.WriteLine(custAddress);
                Order order = new Order(custId, custAddress);
                SqlConnection connection = GetConnection(SqlStr);

                using (connection)
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        // Insert into orders
                        string orderQuery = "INSERT INTO orders (CustomerId, OrderDate, DeliveryAddress, DeliverStatus) VALUES (@custId, @orDate, @custAddy, @status); SELECT SCOPE_IDENTITY();";
                        SqlCommand orderCommand = new SqlCommand(orderQuery, connection, transaction);
                        orderCommand.Parameters.AddWithValue("@custId", order.CustomerId);
                        orderCommand.Parameters.AddWithValue("@orDate", order.OrderDate);
                        orderCommand.Parameters.AddWithValue("@custAddy", order.DeliveryAddress);
                        orderCommand.Parameters.AddWithValue("@status", order.DeliveryStatus);

                        int resultOrderId = Convert.ToInt32(orderCommand.ExecuteScalar());
                        System.Console.WriteLine($"DBCreateOrder transaction says orderid is {resultOrderId}");

                        if (resultOrderId != 0)
                        {
                            // Call DBcreateOrderItem within the same transaction context
                            int orderItemId = DBcreateOrderItem(resultOrderId, itemId, quantity, connection, transaction);
                            if (orderItemId != 0)
                            {
                                transaction.Commit();
                                System.Console.WriteLine($"Transaction committed");
                                return resultOrderId;
                            }
                            else
                            {
                                throw new Exception("Failed to create order item");
                            }
                        }
                        else
                        {
                            throw new Exception("Order ID generation failed");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error: " + ex.Message);
                        transaction?.Rollback();
                        return 0;
                    }
                }
            }

            public override int DBcreateOrderItem(int orderId, int itemId, int quantity, SqlConnection connection, SqlTransaction transaction) //returns orderItemId
            {
                System.Console.WriteLine($"this is DBCreateOrderItem");

                OrderItem orItem = new(orderId, itemId, quantity);
                decimal price = GetItemPrice(orItem.ItemId);
                decimal totalPrice = price * orItem.Quantity;
                decimal roundTotalPrice = Math.Round(totalPrice);
                System.Console.WriteLine($"totalPrice is {roundTotalPrice}");

                string query = "INSERT INTO orderItems (OrderId, ItemId, Quantity, Price) VALUES (@orderId, @itemId, @quant, @price); SELECT SCOPE_IDENTITY();";
                SqlCommand command = new SqlCommand(query, connection, transaction);
                command.Parameters.AddWithValue("@orderId", orItem.OrderId);
                command.Parameters.AddWithValue("@itemId", orItem.ItemId);
                command.Parameters.AddWithValue("@quant", orItem.Quantity);
                command.Parameters.AddWithValue("@price", roundTotalPrice);

                int resultOrderItemId = Convert.ToInt32(command.ExecuteScalar());
                System.Console.WriteLine($"DBCreateOrderItem returning OrderItemId {resultOrderItemId}");
                return resultOrderItemId;
            }

            public override decimal GetItemPrice(int itemId)
            {
                System.Console.WriteLine($"this is GetItemPrice with itemId {itemId}");
                using (SqlConnection connection = GetConnection(SqlStr))
                {
                    String sql = "SELECT ItemPrice FROM inventory WHERE ItemId = @itemId";
                    using (SqlCommand command = new(sql, connection))
                    {
                        command.Parameters.AddWithValue("@itemId", itemId);
                        connection.Open();
                        decimal resultItemPrice = Convert.ToDecimal(command.ExecuteScalar());
                        return resultItemPrice;
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
            public String GetCustomerAddress(int custId, SqlConnection connection = null, SqlTransaction transaction = null)
            {
                System.Console.WriteLine($"GetCustomerAddress says custId is:{custId}");
                bool isConnectionInternallyManaged = false;
                if (connection == null)
                {
                    connection = GetConnection(SqlStr);
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    isConnectionInternallyManaged = true;
                }
                try
                {
                    using (connection)
                    {
                        String sqlCommand = "SELECT HomeAddress FROM customer WHERE CustomerId = @custId";
                        using (SqlCommand command = new(sqlCommand, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@custId", custId);
                            connection.Open();
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    String resultAddress = reader.GetString(0);
                                    if (isConnectionInternallyManaged)
                                    {
                                        // Commit transaction and close connection if managed internally
                                        transaction.Commit();
                                        connection.Close();
                                    }
                                    return resultAddress;
                                }
                                else
                                {
                                    System.Console.WriteLine("nothing to read here");
                                    return null;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    if (isConnectionInternallyManaged && transaction != null)
                    {
                        // Attempt to roll back the transaction if there was an error and it's managed internally
                        transaction.Rollback();
                        connection.Close();
                    }
                    throw; // Re-throw the exception
                }
            }

            public List<List<String>> DBListOrders(int custId)  //lists orders based on CustomerId. each List contains an orderId, address and status
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    SqlCommand command = new();
                    StringBuilder sqlStr = new StringBuilder("SELECT OrderId, DeliveryAddress,DeliverStatus FROM orders");
                    if (custId != 0)  //admin value
                    {
                        sqlStr.Append(" WHERE CustomerId = @customer");
                        command.Parameters.AddWithValue("@customer", custId);
                    }
                    command.Connection = connection;
                    command.CommandText = sqlStr.ToString();
                    using (command)
                    {
                        connection.Open();
                        SqlDataReader read = command.ExecuteReader();
                        List<List<string>> orderList = new();
                        int orderCount = 0;
                        while (read.Read())
                        {
                            List<string> orderDetail = new List<string>
                            {
                                read.GetInt32(0).ToString(),//order id
                                read.GetString(1),          //address
                                read.GetString(2)           //status
                            };
                            orderList.Add(orderDetail);
                        }
                        return orderList;
                    }
                }
            }
            public List<String> DBListOrderItems(int orderId)  // lists an OrderId's items (ItemIds)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    String sqlStr = "SELECT ItemId FROM orderItems WHERE OrderId = @orderId";
                    SqlCommand command = new(sqlStr, connection);
                    command.Parameters.AddWithValue("@orderId", orderId);
                    using (command)
                    {
                        connection.Open();
                        SqlDataReader read = command.ExecuteReader();
                        List<string> itemList = new();
                        while (read.Read())
                        {
                            itemList.Add(Convert.ToString(read[0]));
                        }
                        return itemList;
                    }
                }
            }
            public override List<(int ItemId, String ItemName)> DBListItems() // dispays all items in inventory in tuples (itemId, itemName)
            {
                SqlConnection connecti = GetConnection(SqlStr);
                using (connecti)
                {
                    String sql = "SELECT ItemId, ItemName FROM inventory;"; //, ItemDescr, ItemPrice FROM inventory;";
                    using (SqlCommand command = new SqlCommand(sql, connecti))
                    {
                        connecti.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                int rowCount = 0;
                                List<(int, string)> idAndName = new();

                                /*var IdAndName = new List<object>  // list of lists
                                {
                                    new List<int>(),                            // list of ItemId
                                    new List<string>()                          // list of ItemName
                                };*/
                                while (reader.Read())
                                {
                                    //int conv = Convert.ToInt32(reader["ItemId"]);
                                    //((List<int>)IdAndName[0]).Add(conv);  // adds 
                                    //((List<string>)IdAndName[1]).Add((String)reader["ItemName"]);
                                    int itmId = Convert.ToInt32(reader["ItemId"]);
                                    string itmName = reader["ItemName"].ToString();
                                    idAndName.Add((itmId, itmName));
                                }
                                return idAndName;
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

            public override int DBCreateMember(Member memb)
            {
                SqlConnection connection = GetConnection(SqlStr);
                using (connection)
                {
                    //SQL STRING BUILD
                    StringBuilder sb = new();
                    sb.Append("USE master; ");
                    sb.Append("INSERT INTO member (Login, PasswordHash, Salt, Role) VALUES ");
                    sb.Append("(@login, @passHash, @salt, @role);");
                    sb.Append("SELECT SCOPE_IDENTITY();");
                    String sql = sb.ToString();

                    // HASH AND SALT
                    Byte[] salt = SecurityHelper.CreateSalt();
                    String passHash = SecurityHelper.HashPassword(memb.pass, salt);
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@login", memb.login);
                        command.Parameters.AddWithValue("@passHash", passHash);
                        command.Parameters.AddWithValue("@salt", salt);
                        command.Parameters.AddWithValue("@role", 1);

                        connection.Open();
                        int resultMemId = Convert.ToInt32(command.ExecuteScalar());
                        Console.WriteLine($"member id is {resultMemId}.");
                        return resultMemId;
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
                        command.Parameters.AddWithValue("@Coords", cust.Coordinates);
                        command.Parameters.AddWithValue("@date", cust.DateCreated);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine(rowsAffected + " row(s) inserted");
                        return rowsAffected > 0;
                    }
                }
            }
            public override void DBAddItem(Item item)
            {
            }
            public override void DBDeleteItem(string id)
            {
            }

        }
    }
}
