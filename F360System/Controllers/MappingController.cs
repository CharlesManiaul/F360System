using Azure;
using Dapper;
using F360System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;

namespace F360System.Controllers
{
    public class MappingController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;
        public MappingController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }
        public IActionResult Index()
        {
            ViewBag.map = "active";
            string sql;
            Mapping mapping = new Mapping();
            var eid = HttpContext.User.FindFirstValue("eid");
            mapping.map = db.Query<Mapping>("sp_360_EvalHeader_List");

            mapping.EmpList = db.Query<EmpMaster>("SELECT eid EId, name EmpName FROM employee_master");
            mapping.FormHeader = db.Query<Questionaire>("SELECT * FROM F360_FormHeader");

           



            return View(mapping);
        }

        [Route("/Mapping/Details/{EVId}")]
        public IActionResult Details(int EVId)
        {
            ViewBag.map = "active";
            string sql;
            Mapping mapping = new Mapping();
            var name = HttpContext.User.FindFirstValue(ClaimTypes.Name);

            mapping = db.QueryFirst<Mapping>("sp_360_EvalHeader_Select", new { EVId });

            mapping.MapDetails = db.Query<MapDetails>(@"Select a.EvID, a.Relation,a.EvID,b.name as RateeName, a.RateeID Ratee from F360_EvalMapping a
                                                        JOIN employee_master b
                                                        on a.RateeID = b.eid
                                                        where EVId = @EVId", new { EVId });

            mapping.EmpList = db.Query<EmpMaster>("SELECT eid EId, name EmpName FROM employee_master where name <> @EmpName and IsActive = 1", new { mapping.EmpName });

            return View(mapping);
        }


        //header save
        public IActionResult AddEvalHeader(Mapping mapping)
        {
            var response = db.ExecuteScalar("sp_360_EvalHeader_Add", new
            {
                mapping.HId,
                mapping.EId,
                mapping.Description,
                mapping.Remarks,
                mapping.DateFrom,
                mapping.DateTo,
                mapping.DeadLine,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name)
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added New Evaluation Header";
                TempData["Success Message"] = "Header has been Added.";
                return RedirectToAction("Index");

            }

            TempData["Error Title"] = "New Evaluation HeaderFailed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index");

        }




        //Mapping save
        public IActionResult SaveMapping(Mapping mapping)
        {
            var response = db.ExecuteScalar("sp_360_EvalMapping_Add", new
            {
                mapping.EVId,
                mapping.mapDetails.Relation,
                mapping.mapDetails.Ratee,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Added employee mapping";
                TempData["Success Message"] = "Employee mapping has been Added.";
                return RedirectToAction("Details", new { mapping.EVId });

            }

            TempData["Error Title"] = "Employee mapping failed to add";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { mapping.EVId });

        }

        //Mapping save
        public IActionResult EditMapping(Mapping mapping)
        {
            var response = db.ExecuteScalar("sp_360_EvalMapping_Edit", new
            {
                mapping.EVId,
                mapping.mapDetails.Relation,
                mapping.mapDetails.Ratee,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully edited employee mapping";
                TempData["Success Message"] = "Mapping has been updated.";
                return RedirectToAction("Details", new { mapping.EVId });

            }

            TempData["Error Title"] = "Employee mapping Failed to edit";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { mapping.EVId });

        }
        //Mapping Delete
        public IActionResult DeleteMapping(Mapping mapping)
        {
            var response = db.ExecuteScalar("sp_360_EvalMapping_Delete", new
            {
                mapping.EVId,
                mapping.mapDetails.Ratee,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Deleted from Mapping Header";
                TempData["Success Message"] = "Map has been Deleted.";
                return RedirectToAction("Details", new { mapping.EVId });

            }

            TempData["Error Title"] = "Deletion of Mapping failed";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Details", new { mapping.EVId });

        }


        public IActionResult DeleteEvalHeader(Mapping mapping)
        {
            var response = db.ExecuteScalar("sp_360_EvalHeader_Delete", new
            {
                mapping.EVId,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
            }, commandType: System.Data.CommandType.StoredProcedure);


            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Deleted from Mapping Header";
                TempData["Success Message"] = "Map has been Deleted.";
                return RedirectToAction("Index");

            }

            TempData["Error Title"] = "Deletion of Mapping failed";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index");

        }




        public IActionResult EditEvalHeader(Mapping mapping) 
        {
            var response = db.ExecuteScalar("sp_360_EvalHeader_Edit", new
            {
                mapping.EVId,
                mapping.Description,
                mapping.Remarks,
                mapping.DateFrom,
                mapping.DateTo,
                mapping.DeadLine,
                CrtdBy = HttpContext.User.FindFirstValue(ClaimTypes.Name),
                mapping.AcknowledgeBy

            }, commandType: System.Data.CommandType.StoredProcedure);

            if (response.ToString() == "Successful") // Assuming the query will affect only one row
            {
                TempData["Success Title"] = "Successfully Edit Evaluation Header";
                TempData["Success Message"] = "Header has been Edited.";
                return RedirectToAction("Index");

            }

            TempData["Error Title"] = "Evaluation Header failed to edit";
            TempData["Error Message"] = response.ToString();
            return RedirectToAction("Index");
        }







        public JsonResult GetHead(string EVId)
        {
            var headList = db.Query<EmpMaster>(@"
                                                 SELECT b.name AS EmpName, a.RateeID AS EId 
                                                 FROM F360_EvalMapping a
                                                 JOIN employee_master b ON a.RateeID = b.eid
                                                 WHERE EvID = @EVId and Relation = 'Head'
                                                ", new { EVId });

            return Json(headList);
        }

    }
}
