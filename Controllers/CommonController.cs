using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Heave.Models;
using System.Collections;

namespace Heave.Controllers;

public class CommonController : Controller
{
    private readonly IConfiguration _configuration;
    public CommonController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    Program.UserSession uSession = new(1,1);
    private Program.userConnect InitUserConnect()//handles sql init for adminConnect methods
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.userConnect userConnect = new();
        userConnect.SqlStr = connectionString;
        return userConnect;
    }

    /*public IActionResult DynamicForm(string formType)
    {
        DynamicFormModel form = new DynamicFormModel { FormType = formType };
        switch (form.FormType)
        {
            case "InputCreateMember":
                {
                    form.FieldInputs.Add("Login", "");
                    form.FieldInputs.Add("Password", "");
                    break;
                }
            default:
                {
                    break;
                }
        }
        return View();
    }*/

    public IActionResult CreateMember()
    {
        DynamicFormModel form = new DynamicFormModel { FormType = "InputCreateMember", FieldInputs = new Dictionary<string, string> { { "Login", "" }, { "Password", "" } } };
        string firstKey = form.FieldInputs.Keys.FirstOrDefault();
        System.Console.WriteLine($" form type: {form.FormType}, fieldInput: {firstKey}");
        return View("CreateMember",form);
    }


    public IActionResult CreateCustomer()
    {
        return View();
    }
    
    public IActionResult Products()
    {
        Program.userConnect userConnect = InitUserConnect();
        List<(int, String)> productList = userConnect.DBListItems();
        return View(productList);
    }

    public IActionResult MyOrders()
    {
        Program.userConnect userConnect = InitUserConnect();
        List<List<string>> orderList = userConnect.DBListOrders(uSession.MemberId);  //maybe change to adminConnect for all order list
        ViewBag.Message = uSession.MemberId;
        return View("Orders",orderList);
    }

    [HttpPost]
    public IActionResult CreateOrder(int itemId,int quantity)
    {
        Program.userConnect userConnect = InitUserConnect();
        int orderId = userConnect.DBCreateOrder(uSession.CustomerId, itemId, quantity);
        ViewBag.Message = orderId != null ? $"Created order number {orderId}" : "There was a problem";
        return View("Confirmation");
    }
        public IActionResult DynamicForm(DynamicFormModel form)
    {
        // Handle the submitted data here
        // You can switch based on model.FormType to handle different form submissions
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        Program.userConnect userConnect = new();
        userConnect.SqlStr = connectionString;
        ///getting input field values by name
        ///
        System.Console.WriteLine(form);
        string login;
        string password;
        if (form.FieldInputs.TryGetValue("Login", out login) && form.FieldInputs.TryGetValue("Password", out password))
        {
            Member member = new Member(login, password);
            int memId = userConnect.DBCreateMember(member);
            ViewBag.Message = $"Member {memId} was created";
        }
          ViewBag.Message = "Form submitted successfully.";

        return RedirectToAction("Confirmation", "Common");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
