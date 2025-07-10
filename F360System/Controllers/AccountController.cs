

using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Data;
using System.Diagnostics;


namespace F360System.Controllers
{
    

    public class AccountController : Controller
    {
        SqlConnection db;
        public AccountController(IConfiguration configuration)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

        }
        public IActionResult Login()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password)
        {

            var result = db.QueryFirstOrDefault("sp_360_login", new { Username, Password }, commandType: CommandType.StoredProcedure);



            if (result is null)

            {
                TempData["error"] = "INVALID USERNAME OR PASSWORD";
                TempData["invalid"] = "is-invalid";
                return RedirectToAction("Login", "Account");

            }

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, result.Name));
            claims.Add(new Claim("position", result.position));
            claims.Add(new Claim("username", result.UserName.ToString()));
            claims.Add(new Claim("department", result.depName));
            claims.Add(new Claim("eid", result.eid));


            if (result.eid == "09930" || result.eid == "07826" || result.eid == "11942")
            {
                claims.Add(new Claim("Admin", "Admin"));
            }

            if (result.eid == "09484" || result.eid == "01118")
            {
                claims.Add(new Claim("Executive", "Executive"));
            }


            if (result.eid != "09930" && result.eid != "07826" || result.eid != "11942")
            {
                claims.Add(new Claim("User", "User"));
            }


            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };

            






            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // If either HumanResources or Training is 1, redirect to the "Index" action in the "Home" controller
            return RedirectToAction("Index", "Ratee");



        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match.";
                return RedirectToAction("Index", "Ratee");
            }

            var result = db.ExecuteScalar("sp_360_Change_Password", new
            {
                Username = HttpContext.User.FindFirstValue("eid"),
                OldPassword,
                NewPassword,
                ConfirmPassword
            }, commandType: CommandType.StoredProcedure);

            if (result != null && result.ToString() == "Successful")
            {
                TempData["Message"] = "Password changed! Please log in again.";

                // Sign out user
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Index", "Home"); // or Login page
            }
            else
            {
                TempData["ErrorMessage"] = result?.ToString() ?? "An error occurred.";
                return RedirectToAction("Index", "Ratee");
            }
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            return RedirectToAction("Index", "Home");
        }



    }
}
