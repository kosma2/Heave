namespace Heave
{
    class OrderItem
    {
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public OrderItem(int orderId, int itemId, int quant)
        {
            OrderId = orderId;
            ItemId = itemId;
            Quantity = quant;
            //Price = userConnect.GetItemPrice(itemId);
        }
    }
}