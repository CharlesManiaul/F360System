using Dapper;
using F360System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace F360System.Controllers
{
    public class QuestionaireController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;

        public QuestionaireController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }
        //Header index
        public IActionResult Index()
        {
            string sql;
            Questionaire quest = new Questionaire();
            ViewBag.question = "active";
            quest.QuestionHeaders = db.Query<Questionaire>("sp_360_FormHeader_List");


            return View(quest);
        }
        //Header Details
        [Route("/Questionaire/Details/{HId}")]
        public IActionResult Details(int HId)
        {
            string sql;
            Questionaire quest = new Questionaire();
            ViewBag.question = "active";
            quest = db.QueryFirst<Questionaire>("sp_360_FormHeader_Select", new { HId });

            quest.QuestionDetails = db.Query<QuestionaireDetails>("sp_360_FormDetails_List", new { HId });

            quest.categories = db.Query<string>("SELECT DISTINCT Category from F360_FormDetails").ToArray();

            return View(quest);
        }




        //saving header
        public IActionResult SaveHeader(Questionaire quest)
        {


            var response = db.ExecuteScalar("sp_360_FormHeader_Add", new
            {
                quest.FormName,
                quest.Remarks,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added New Form";
                TempData["Success Message"] = "Form has been Added.";
                return RedirectToAction("Index", "Questionaire");

            }

            TempData["Error Title"] = "New Form Failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index", "Questionaire");

        }

        //saving QuestionaireDetails


        public IActionResult SaveQuestionDetails(Questionaire quest)
        {


            var response = db.ExecuteScalar("sp_360_FormDetails_Add", new
            {
                quest.HId,
                quest.QuestionDetail.Category,
                quest.QuestionDetail.Seq,
                quest.QuestionDetail.Questionaire,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added New Form";
                TempData["Success Message"] = "Form has been Added.";
                return RedirectToAction("Details", new { quest.HId });

            }

            TempData["Error Title"] = "New Form Failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { quest.HId });

        }

        public IActionResult EditQuestionDetails(Questionaire quest)
        {


            var response = db.ExecuteScalar("sp_360_FormDetails_Edit", new
            {
                quest.QuestionDetail.DId,
                quest.HId,
                quest.QuestionDetail.Category,
                quest.QuestionDetail.Seq,
                quest.QuestionDetail.Questionaire,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added New Form";
                TempData["Success Message"] = "Form has been Added.";
                return RedirectToAction("Details", new { quest.HId });

            }

            TempData["Error Title"] = "New Form Failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { quest.HId });

        }



        //Delete header
        public IActionResult DeleteHeader(Questionaire quest)
        {


            var response = db.ExecuteScalar("sp_360_FormHeader_Delete", new
            {
                quest.HId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Deleted Form";
                TempData["Success Message"] = "Form has been deleted.";
                return RedirectToAction("Index", "Questionaire");

            }

            TempData["Error Title"] = "Form Failed to delete";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index", "Questionaire");

        }





        //Delete header
        public IActionResult DeleteQuestionDetails(Questionaire quest)
        {


            var response = db.ExecuteScalar("sp_360_FormDetails_Delete", new
            {
          
                quest.QuestionDetail.DId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            });


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Deleted Question";
                TempData["Success Message"] = "Question has been deleted.";
                return RedirectToAction("Details", new { quest.HId});

            }

            TempData["Error Title"] = "Question Failed to delete";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { quest.HId });


        }









    }
}
