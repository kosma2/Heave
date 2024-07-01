namespace Heave
{
    public class Member
    {
        public int MemberId { get; set;}
        public string login { get; set; } = "";
        public string pass { get; set; } = "";
        public int role {get; set;} = 1; // user role 0=admin, 1=user
        
        public Member(String log, String passWord)
        {
            login = log;
            pass = passWord;
        }
        public Member()
        {}
    }
}
