namespace Heave
{
    partial class Program
    {
        public abstract class DbConnection
        {

            public String SqlStr;
            //public abstract void Connect(String connString);
            public abstract string GetItemName(int itemId);
            public abstract int DBGetCustomerId(int memId); // retrieves MemberId from customer table

            public abstract void DBdeleteOrder(int orderId);
            public abstract (int,int) InterfaceCreateOrder();
            public abstract int DBCreateOrder(int custId, int itemId, int quantity);
            public abstract int DBcreateOrderItem(int orderId, int itemId, int quantity);
            public abstract List<(int ItemId, String ItemName)> DBListItems();
            public abstract void DBAddItem(Item item);
            public abstract void DBDeleteItem(String id);
            public abstract int DBCreateMember(Member mem);
            public abstract bool DBCreateCustomer(Customer cust);
            public abstract int DBCheckLogin(Member mem);
            public abstract decimal GetItemPrice(int itemId);
             public abstract List<(int,string)> ShowCustomers();

        }
    }
}
