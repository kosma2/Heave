using System.ComponentModel.DataAnnotations;

public class MemberLoginModel
{
   // [Required]
    //[Display(Name = "Username")]
    public string Username { get; set; }

    //[Required]
    //[DataType(DataType.Password)]
    //[Display(Name = "Password")]
    public string Password { get; set; }
}