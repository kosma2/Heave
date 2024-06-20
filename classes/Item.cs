namespace Heave
{
    class Item
    {
        public String itemId { get; set; } = "";
        public String itemName { get; set; } = "";
        public String itemDesc { get; set; } = "";
        public String itemPrice { get; set; } = "";
        public String itemDimens { get; set; } = "";
        public String itemWeight { get; set; } = "";
        public Item(String id, String nam, String des, String pric, String dims, String weit)
        {
            itemId = id;
            itemName = nam;
            itemDesc = des;
            itemPrice = pric;
            itemDimens = dims;
            itemWeight = weit;
        }
    }
}
