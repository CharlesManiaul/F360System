using F360System.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using Dapper;
using System.Security.Claims;
using System.Data;

using FastReport.Export.PdfSimple;
using FastReport;
using MySqlX.XDevAPI.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace F360System.Controllers
{

    public class AcknowledgeController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;
        private readonly object configuration;

        public AcknowledgeController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }



        public IActionResult Index()
        {
            string sql;
            Acknowledge acknowledge = new Acknowledge();
            ViewBag.acknowledge = "active";
            var username = HttpContext.User.FindFirstValue("eid");

            if (User.HasClaim("Admin", "Admin"))
            {
                acknowledge.acknowledge = db.Query<Acknowledge>(@"
                                                                SELECT 
                                                                a.EVId, 
                                                                a.EID,
                                                                a.Remarks, 
                                                                a.DateFrom, 
                                                                a.DateTo, 
                                                                a.DeadLine, 
                                                                b.name AS EmpName, 
                                                                c.name AS AcknowledgeBy,
                                                                a.AcknowledgeDate,
                                                                a.AcknowledgeRemarks,
                                                                CASE 
                                                                WHEN a.AcknowledgeRemarks IS NULL OR LTRIM(RTRIM(a.AcknowledgeRemarks)) = '' THEN 'Pending'
                                                                WHEN a.EId = a.ConfirmBy THEN 'Completed'
                                                                ELSE 'For Discussion'
                                                                END AS AcknowledgeStatus
                                                                FROM F360_EvalHeader a
                                                                JOIN employee_master b ON a.EID = b.EID
                                                                JOIN employee_master c ON a.AcknowledgeBy = c.EID
                                                                WHERE a.AcknowledgeBy IS NOT NULL");

            }
            else 
            {


                acknowledge.acknowledge = db.Query<Acknowledge>(@"
                                                                SELECT 
                                                                a.EVId, 
                                                                a.EID,
                                                                a.Remarks, 
                                                                a.DateFrom, 
                                                                a.DateTo, 
                                                                a.DeadLine, 
                                                                b.name AS EmpName, 
                                                                c.name AS AcknowledgeBy,
                                                                a.AcknowledgeDate,
                                                                a.AcknowledgeRemarks,
                                                                CASE 
                                                                    WHEN a.AcknowledgeRemarks IS NULL OR LTRIM(RTRIM(a.AcknowledgeRemarks)) = '' THEN 'Pending'
                                                                    WHEN a.EId = a.ConfirmBy THEN 'Completed'
                                                                    ELSE 'For Discussion'
                                                                END AS AcknowledgeStatus
                                                                FROM F360_EvalHeader a
                                                                JOIN employee_master b ON a.EID = b.EID
                                                                JOIN employee_master c ON a.AcknowledgeBy = c.EID
                                                                WHERE  
                                                                (
                                                                    a.AcknowledgeBy IS NOT NULL 
                                                                    AND a.EID = @username 
                                                                    AND (
                                                                        CASE 
                                                                            WHEN a.AcknowledgeRemarks IS NULL OR LTRIM(RTRIM(a.AcknowledgeRemarks)) = '' THEN 'Pending'
                                                                            WHEN a.AcknowledgeBy = a.ConfirmBy THEN 'Completed'
                                                                            ELSE 'For Discussion'
                                                                        END
                                                                    ) IN ('For Discussion', 'Completed')
                                                                )
                                                                OR (a.AcknowledgeBy = @username);
                                                                ", new { username});

            }







                return View(acknowledge);
        }


        [Route("/Acknowledge/Details/{EVId}")]
        public IActionResult Details(string EVId)
        {
            string sql;
            Acknowledge acknowledge = new Acknowledge();
            ViewBag.acknowledge = "active";

            acknowledge = db.QueryFirst<Acknowledge>(@" SELECT 
                                                        a.EVId, 
                                                        a.EId,
                                                        a.Remarks, 
                                                        a.DateFrom, 
                                                        a.DateTo, 
                                                        a.DeadLine, 
                                                        b.name AS EmpName, 
                                                        c.name AS AcknowledgeBy,
                                                        a.AcknowledgeDate,
                                                        a.AcknowledgeRemarks,
                                                        a.ConfirmRemarks,
                                                        CASE 
                                                        WHEN a.AcknowledgeRemarks IS NULL OR LTRIM(RTRIM(a.AcknowledgeRemarks)) = '' THEN 'Pending'
                                                        WHEN a.EId = a.ConfirmBy THEN 'Completed'
                                                        ELSE 'For Discussion'
                                                        END AS AcknowledgeStatus
                                                        FROM F360_EvalHeader a
                                                        JOIN employee_master b ON a.EID = b.EID
                                                        JOIN employee_master c ON a.AcknowledgeBy = c.EID
                                                        WHERE a.AcknowledgeBy IS NOT NULL and a.EVId = @EVId", new { EVId });





            var rawRemarks = db.Query<PrintRemarks>(
                            "sp_360_GetName_Remarks",
                            new { EVId },
                            commandType: CommandType.StoredProcedure
                            ).ToList();




            var groupedRemarks = rawRemarks
            .GroupBy(r => r.Category)
            .Select(g => new GroupedRemark
            {
                Category = g.Key,
                SubGroups = g
                    .GroupBy(r => r.Relation)
                    .Select(sub => new RelationGroup
                    {
                        Relation = sub.Key,
                        Remarks = sub.ToList()
                    }).ToList()
            }).ToList();




            acknowledge.printRemarks = rawRemarks;
            acknowledge.GroupedRemarks = groupedRemarks;



            var rawRates = db.Query<PrintRate>(
                            "sp_360_GetName",
                            new { EVId },
                            commandType: CommandType.StoredProcedure
                                 ).ToList();

            var grouped = rawRates
                .GroupBy(r => r.Category)
                .Select(g => new PrintRate
                {
                    Category = g.Key,
                    Items = g.OrderBy(r => r.Seq).ToList(),
                    AvgHead = g.Average(r => (double)r.Head),
                    AvgPeers = g.Average(r => (double)r.Peers),
                    AvgCustomer = g.Average(r => (double)r.InternalCustomer),
                    AvgDR = g.Average(r => (double)r.DirectReport),
                    AvgSelf = g.Average(r => (double)r.Self),
                    AvgAllGroup = g.Average(r => (double)r.AvgAll),
                    AvgNoSelfGroup = g.Average(r => (double)r.AvgNS)
                }).ToList();


            var allItems = rawRates;

            var overall = new PrintRate
            {
                AvgHead = allItems.Average(r => (double)r.Head),
                AvgPeers = allItems.Average(r => (double)r.Peers),
                AvgCustomer = allItems.Average(r => (double)r.InternalCustomer),
                AvgDR = allItems.Average(r => (double)r.DirectReport),
                AvgSelf = allItems.Average(r => (double)r.Self),
                AvgAllGroup = allItems.Average(r => (double)r.AvgAll),
                AvgNoSelfGroup = allItems.Average(r => (double)r.AvgNS)
            };


            acknowledge.GroupedRatings = grouped;
            acknowledge.Overall = overall;

            acknowledge.printRate = grouped;





            return View(acknowledge);

        }



        public IActionResult SubmitRemarks(Acknowledge acknowledge)
        {
            var response = db.ExecuteScalar("sp_360_Acknowledge_Map", new
            {
                acknowledge.AcknowledgeRemarks,
                acknowledge.EVId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Acknowledge the mapping";
                TempData["Success Message"] = "Acknowledge has been Added.";
                return RedirectToAction("Details", new { acknowledge.EVId});

            }

            TempData["Error Title"] = "Acknowledge the mapping failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { acknowledge.EVId });

        }


        public IActionResult ConfirmIdentity(Acknowledge acknowledge)
        {
            var response = db.ExecuteScalar("sp_360_ConfirmIdentity", new
            {
                acknowledge.Confirm.UserId,
                acknowledge.Confirm.password,
                acknowledge.Confirm.ConfirmRemarks,
                acknowledge.EVId,
                acknowledge.AcknowledgeRemarks,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Closed the Acknowledgement";
                TempData["Success Message"] = "Acknowledge has been Closed.";
                return RedirectToAction("Details", new { acknowledge.EVId });

            }

            TempData["Error Title"] = "Closing of Acknowledgement failed";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { acknowledge.EVId });

        }


    }
}
