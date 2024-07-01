using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;

namespace Heave.Controllers;

public class AdminController : Controller
{
    private readonly IConfiguration _configuration;
    public AdminController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    Program.UserSession uSession = new(0,0);//


    private Program.adminConnect InitAdminConnect()//handles sql init for adminConnect methods
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.adminConnect adminConnect = new();
        adminConnect.SqlStr = connectionString;
        return adminConnect;
    }
    public IActionResult Index()
    {
        return View(new MemberLoginModel());
    }

    public IActionResult Customers()
    {
        Program.adminConnect adminConnect = InitAdminConnect();
        List<(int,String)> customerList = adminConnect.ShowCustomers();
        return View(customerList);
    }
    public IActionResult Orders()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.userConnect userConnect = new();
        userConnect.SqlStr = connectionString;
        List<List<string>> orderList = userConnect.DBListOrders(0);  //maybe change to adminConnect for all order list
        ViewBag.Message = "Good job!";
        return View(orderList);
    }
    public IActionResult Members()
    {
        Program.adminConnect adminConnect = InitAdminConnect();
        List<(int, String)> memberList = adminConnect.DBListMembers();
        return View(memberList);
    }
    [HttpGet]
    //[Route("CreateCustomer/{memberId}")]
    public IActionResult CreateCustomer(int memberId)
    {
        System.Console.WriteLine($"Member id is {memberId}");
        Customer customer = new Customer(){MemberId=memberId};
        return View("CreateCustomer",customer);
    }
    
    
    [HttpPost]
    public IActionResult UpdateRole(int memberId,int role)
    {
        Program.adminConnect adminConnect = InitAdminConnect();
        bool success = adminConnect.DBUpdateMemberRole(memberId, role);
        ViewBag.Message = success ? $"Member {memberId}'s role was changed to {role} ." : "something went wrong";
        return View("Confirmation");
    }
    public IActionResult DeleteMember(int memberId)
    {
        Program.adminConnect adminConnect = InitAdminConnect();
        adminConnect.DBDeleteMember(memberId);
        ViewBag.Message = $"Member {memberId} was deleted.";
        return View("Confirmation");
    }
     [HttpPost]
    public IActionResult CreateCustomer(Customer customer)
    {
        System.Console.WriteLine("postCC");
        if (ModelState.IsValid)
        {
            Customer customerNew = new Customer(customer.MemberId,customer.FirstName,customer.LastName,
            customer.HomeAddress,customer.Coordinates);
            Program.adminConnect adminConnect = InitAdminConnect();
            bool success = adminConnect.DBCreateCustomer(customerNew);
            ViewBag.Message = success ? "Customer created" : "There was a problem";
        }

        return View("Confirmation");
    }
    public IActionResult Index(MemberLoginModel model)
    {
        if (ModelState.IsValid)
        {
            Program.adminConnect adminConnect = InitAdminConnect();
            int loginResult = adminConnect.DBCheckLogin(new Member(model.Username, model.Password));
            System.Console.WriteLine(loginResult);
            switch (loginResult)
            {
                case -1:
                    ViewBag.Message = "Password mismatch.";
                    break;
                case -2:
                    ViewBag.Message = "No such user.";
                    break;
                default:
                    ViewBag.Message = "Logged in successfully. Member ID: " + loginResult;
                    break;
            }
        }
        else
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        return View("Index", model);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
