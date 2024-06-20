namespace Heave
{
    class Customer : Member
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string HomeAddress { get; set; } = "";
        public string Coordinates { get; set; } = "";
        public DateTime DateCreated { get; set; }

        public Customer(int memberId,string firName, string lasName, string homeAdd, string coords)
        : base(null, null)
        {
            MemberId = memberId;
            FirstName = firName;
            LastName = lasName;
            HomeAddress = homeAdd;
            Coordinates = coords;
            DateCreated = DateTime.Now;
        }
    }
}
