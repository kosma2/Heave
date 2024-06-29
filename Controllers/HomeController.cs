using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;

namespace Heave.Controllers;

public class HomeController : Controller
{
     private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


    public IActionResult Index()
    {
        
        return View(new MemberLoginModel());
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [HttpPost]
        public IActionResult Index(MemberLoginModel model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                Program.UserSession uSession = null;
                Program.adminConnect adminConnect = new();
                adminConnect.SqlStr = connectionString;
                System.Console.WriteLine("here");
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
           
            return View("Index",model);
        }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
