using Microsoft.AspNetCore.Mvc;
using Dapper;
using FluentFTP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using F360System.Models;

namespace F360System.Controllers
{
    public class CategoryController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;

        public CategoryController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }

        public IActionResult Index()
        {
            string sql;
            Category cat = new Category();

            cat.categories = db.Query<Category>("sp_360_FormCategory_List");


            return View(cat);
        }


        public IActionResult SaveCategory(Category cat) 
        {
           

            var response = db.ExecuteScalar("sp_360_FormCategory_Add", new
            {
                cat.Seq,
                cat.CategoryName,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),    
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added New Category";
                TempData["Success Message"] = "Category has been Added.";
                return RedirectToAction("Index", "Category");

            }

            TempData["Error Title"] = "New Category Failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index", "Category");

        }

    }
}
