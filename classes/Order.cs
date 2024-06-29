namespace Heave
{
    class Order
    {
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public String DeliveryAddress { get; set; } = "";
        public String DeliveryStatus { get; set; } = "begun";

        public Order(int cusId, String addy)
        {
            CustomerId = cusId;
            OrderDate = DateTime.Now;
            DeliveryAddress = addy;
            //DeliveryStatus = status;
        }
    }
}
