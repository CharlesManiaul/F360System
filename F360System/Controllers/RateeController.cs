using F360System.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using Dapper;
using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Authorization;


namespace F360System.Controllers
{
    [Authorize]
    public class RateeController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;
        public RateeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }

        public IActionResult Index()
        {
            ViewBag.rate = "active";
            string sql;
            Mapping map = new Mapping();
           
            var username = HttpContext.User.FindFirstValue("username");

            map.map = db.Query<Mapping>(@"SELECT a.*, c.name AS EmpName, b.Status
                                        FROM F360_EvalHeader a
                                        JOIN F360_EvalMapping b ON a.EvID = b.EvID
                                        JOIN employee_master c ON a.EID = c.eid
                                        WHERE b.RateeID = @username
                                           ", new { username });
            //AND a.deadline >= GETDATE()

            return View(map);
        }
        [Route("/Ratee/Details/{EVId}")]
        public IActionResult Details(int EVId)
        {
            ViewBag.rate = "active";
            string sql;
            EvalResult result = new EvalResult();

            result = db.QueryFirst<EvalResult>(@"SELECT DISTINCT a.EVId,b.name EmpName, c.*,d.* FROM F360_EvalHeader a
                                                 JOIN employee_master b
                                                on a.EID = b.eid
                                                LEFT JOIN F360_EvalResult c
                                                on a.EvID = c.EvId
                                                LEFT JOIN F360_EvalResult_Rating d
                                                on c.Id = d.ERId
                                                Where a.EvID = @EVId", new { EVId });

            result.evalRemarks = db.Query<EvalRemarks>(@"
                                                        SELECT * FROM F360_EvalResult_Remarks a
                                                        JOIN F360_EvalResult b
                                                        on a.ERId = b.Id
                                                        LEFT JOIN F360_EvalResult_Rating c
                                                        on b.Id = c.ERId
                                                        Where b.EvId = @EVId", new { EVId }).ToList();

            result.evalRatings = db.Query<EvalRating>(@"
                                                         SELECT b.*, d.Rate FROM F360_EvalHeader a
                                                        join F360_FormDetails b
                                                        on a.HId = b.HId
                                                        LEFT JOIN F360_EvalResult_Rating d
                                                        on b.DID = d.DID
                                                        Where a.EvID = @EVId", new { EVId }).ToList();



            return View(result);



        }




        [Route("/Ratee/Form/{EVId}")]
        public IActionResult Form(int EVId)
        {
            ViewBag.rate = "active";
            string sql;
            EvalResult result = new EvalResult();
            var username = HttpContext.User.FindFirstValue("username");

            result = db.QueryFirst<EvalResult>(@"SELECT a.EVId, b.name AS EmpName, c.OverRemarks
                                                FROM F360_EvalHeader a
                                                JOIN employee_master b ON a.EID = b.eid
                                                LEFT JOIN F360_EvalResult c ON a.EvID = c.EvId AND c.RateeId = @username
                                                WHERE a.EvID = @EVId", new { EVId, username });

            result.evalRatings = db.Query<EvalRating>(@"
                                                        SELECT b.*, d.Rate FROM F360_EvalHeader a
                                                        join F360_FormDetails b
                                                        on a.HId = b.HId
                                                        LEFT JOIN F360_EvalResult_Rating d
                                                        on b.DID = d.DID and ERId = (SELECT ID FROM F360_EvalResult where EvId = @EVId and RateeId = @username)
                                                        Where EvID = @EVId", new { EVId, username }).ToList();

            result.evalRemarks = db.Query<EvalRemarks>(@"SELECT * FROM F360_EvalResult_Remarks WHERE ERId =  (SELECT ID FROM F360_EvalResult where EvId = @EVId and RateeId = @username)", new { EVId, username }).ToList();
            return View(result);
        }


        [HttpPost]
        public IActionResult SaveRating(EvalResult model, List<string> CategoryRemarks, string OverallRemarks)
        {
            var ratingsTable = new DataTable();
            ratingsTable.Columns.Add("DID", typeof(int));
            ratingsTable.Columns.Add("Rate", typeof(int));

            foreach (var item in model.evalRatings)
            {
                ratingsTable.Rows.Add(item.DId, item.Rate);
            }

            var remarksTable = new DataTable();
            remarksTable.Columns.Add("Category", typeof(string));
            remarksTable.Columns.Add("Remarks", typeof(string));

            var distinctCategories = model.evalRatings
                .Select(x => x.Category)
                .Distinct()
                .ToList();

            for (int i = 0; i < distinctCategories.Count; i++)
            {
                remarksTable.Rows.Add(distinctCategories[i], CategoryRemarks[i]);
            }

            var response = db.ExecuteScalar("sp_360_EvalResult_Add", new
            {
                model.EVId,
                RateeId = HttpContext.User.FindFirstValue("username"),
                model.OverRemarks,
                QuestionRatings = ratingsTable,
                CategoryRemarks = remarksTable,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                username = HttpContext.User.FindFirstValue("username")
        }, commandType: CommandType.StoredProcedure);

            if (response.ToString() == "Successful")
            {
                TempData["Success Title"] = "Successfully Saved Evaluation";
                TempData["Success Message"] = "Evaluation data has been stored.";
                return RedirectToAction("Index");
            }

            TempData["Error Title"] = "Failed to Save Evaluation";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index");
        }




    }
}
