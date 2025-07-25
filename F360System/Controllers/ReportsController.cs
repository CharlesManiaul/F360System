using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using Dapper;
using System.Security.Claims;
using System.Data;
using F360System.Models;
using FastReport.Export.PdfSimple;
using FastReport;
using MySqlX.XDevAPI.Common;
using Microsoft.EntityFrameworkCore.Metadata.Internal;






namespace F360System.Controllers
{
    public class ReportsController : Controller
    {
        SqlConnection db, adb;
        private string _webHostEnvironment;
        private readonly object configuration;

        public ReportsController(IConfiguration configuration, IWebHostEnvironment env)
        {
            db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            _webHostEnvironment = env.WebRootPath;

        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult StatusReport()
        {
            string sql;
            Reports reports = new Reports();
            ViewBag.reports = "active";

            var username = HttpContext.User.FindFirstValue("eid");


            if (username == "09930" || username == "07826" || username == "11942")
            {
                reports.reports = db.Query<Reports>(@"SELECT 
                                                    a.*, 
                                                    b.name AS EmpName, 
                                                    c.DateFrom,
                                                    c.EID,
                                                    em2.name AS RateeName,  
                                                    c.DateTo,
                                                    c.DeadLine,
                                                    d.OverRemarks,
                                                    d.UpdtDate AS AnswerDate
                                                    FROM F360_EvalMapping a
                                                    JOIN employee_master b
                                                    ON a.RateeID = b.eid
                                                    JOIN F360_EvalHeader c
                                                    ON a.EvID = c.EvID
                                                    JOIN employee_master em2
                                                    ON c.EID = em2.EID 
                                                    JOIN F360_EvalResult d
                                                    ON a.EvID = d.EvID 
                                                    AND b.eid = d.RateeId
                                                
                                                    ORDER BY AnswerDate
                                                    ");
            }

            else
            {

                reports.reports = db.Query<Reports>(@"SELECT 
                                                    a.*, 
                                                    b.name AS EmpName, 
                                                    c.DateFrom,
                                                    c.EID,
                                                    em2.name AS RateeName,  
                                                    c.DateTo,
                                                    c.DeadLine,
                                                    d.OverRemarks,
                                                    d.UpdtDate AS AnswerDate
                                                    FROM F360_EvalMapping a
                                                    JOIN employee_master b
                                                    ON a.RateeID = b.eid
                                                    JOIN F360_EvalHeader c
                                                    ON a.EvID = c.EvID
                                                    JOIN employee_master em2
                                                    ON c.EID = em2.EID 
                                                    JOIN F360_EvalResult d
                                                    ON a.EvID = d.EvID 
                                                    AND b.eid = d.RateeId
                                                    WHERE d.RateeId = @username
                                                    ORDER BY AnswerDate
                                                    ", new { username });
            }


            return View(reports);
        }
        public IActionResult ResultsReport()
        {
            string sql;
            Reports reports = new Reports();
            ViewBag.reports = "active";

            var username = HttpContext.User.FindFirstValue("eid");

            if (User.HasClaim("Executive", "Executive") || User.HasClaim("Admin", "Admin"))
            {
                reports.reports = db.Query<Reports>(@"SELECT 
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
                                                    ");
            }
            else
            {

                reports.reports = db.Query<Reports>(@"
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
                                                WHERE a.AcknowledgeBy IS NOT NULL and a.EID = @username", new { username });

            }

            return View(reports);
        }

        public IActionResult RemarksReport()
        {
            return View();
        }

        public IActionResult PrintAllRemarks()
        {
            // Build the absolute path to the report file.
            string reportPath = Path.Combine(_webHostEnvironment, "reports", "PrintRemarks.frx");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found.", reportPath);
            }

            // Execute the stored procedure to fetch data for the report
            db.Execute("sp_F360_Print_Remarks");

            // Load and prepare the report
            Report report = new Report();
            report.Load(reportPath);

            // Register data here if needed (e.g., report.RegisterData(...))

            report.Prepare();

            // Export the prepared report to a PDF in memory
            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report, ms);
                ms.Position = 0;

                return File(ms.ToArray(), "application/pdf", "AllRemarks.pdf");
            }
        }

        public IActionResult PrintAllRate()
        {
            // Build the absolute path to the report file.
            string reportPath = Path.Combine(_webHostEnvironment, "reports", "PrintRate.frx");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found.", reportPath);
            }

            // Execute the stored procedure to fetch data for the report
            db.Execute("sp_F360_Print_Rate");

            // Load and prepare the report
            Report report = new Report();
            report.Load(reportPath);

            // Register data here if needed (e.g., report.RegisterData(...))

            report.Prepare();

            // Export the prepared report to a PDF in memory
            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report, ms);
                ms.Position = 0;

                return File(ms.ToArray(), "application/pdf", "AllRate.pdf");
            }
        }




        [Route("/PrintSingleRemarks/{Name}")]
        public IActionResult PrintSingleRemarks(string Name)
        {
            // Build the absolute path to the report file.
            string reportPath = Path.Combine(_webHostEnvironment, "reports", "PrintRemarks.frx");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found.", reportPath);
            }

            // Execute the stored procedure to fetch data for the report
            db.Execute("sp_F360_Print_Remarks", new { Name }, commandType: CommandType.StoredProcedure);

            // Load and prepare the report
            Report report = new Report();
            report.Load(reportPath);

            // Register data here if needed (e.g., report.RegisterData(...))

            report.Prepare();

            // Export the prepared report to a PDF in memory
            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report, ms);
                ms.Position = 0;

                return File(ms.ToArray(), "application/pdf", Name + " Remarks.pdf");
            }
        }


        [Route("/PrintSingleRate/{Name}")]
        public IActionResult PrintSingleRate(string Name)
        {
            // Build the absolute path to the report file.
            string reportPath = Path.Combine(_webHostEnvironment, "reports", "PrintRate.frx");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new FileNotFoundException("Report file not found.", reportPath);
            }

            // Execute the stored procedure to fetch data for the report
            db.Execute("sp_F360_Print_Rate", new { Name }, commandType: CommandType.StoredProcedure);

            // Load and prepare the report
            Report report = new Report();
            report.Load(reportPath);

            // Register data here if needed (e.g., report.RegisterData(...))

            report.Prepare();

            // Export the prepared report to a PDF in memory
            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.Export(report, ms);
                ms.Position = 0;

                return File(ms.ToArray(), "application/pdf", Name + " Rate.pdf");
            }
        }


        public IActionResult AcknowledgementReport()
        {
            string sql;
            Reports reports = new Reports();

            reports.reports = db.Query<Reports>(@"SELECT 
                                                a.EVId, 
                                                a.EID,
                                                a.Remarks, 
                                                a.DateFrom, 
                                                a.DateTo, 
	                                                a.DeadLine, 
                                                b.name AS EmpName, 
                                                c.name As AcknowledgeBy,
                                                a.AcknowledgeDate,
                                                CASE 
                                                    WHEN a.AcknowledgeRemarks IS NULL OR LTRIM(RTRIM(a.AcknowledgeRemarks)) = '' THEN 'Pending'
                                                    ELSE 'For Discussion'
                                                END AS AcknowledgeStatus
                                                FROM F360_EvalHeader a
                                                JOIN employee_master b ON a.EID = b.EID
                                                join employee_master c on a.AcknowledgeBy = c.eid
                                                where a.AcknowledgeBy IS NOT NULL");

            return View(reports);

        }



        public IActionResult MappingReport()
        {
            string sql;
            Reports reports = new Reports();
            var eid = HttpContext.User.FindFirstValue("eid");
            reports.map = db.Query<Mapping>("sp_360_Mapping_Report", new {eid });

            return View(reports);

        }



        [Route("/Report/MappingReportDetails/{EVId}")]
        public IActionResult MappingReportDetails(int EVId)
        {
            string sql;
            Reports reports = new Reports();

            reports = db.QueryFirst<Reports>("sp_360_EvalHeader_Select", new { EVId });

            reports.mapDetails = db.Query<MapDetails>(@"Select a.EvID, a.Relation,a.EvID,b.name as RateeName, a.RateeID Ratee from F360_EvalMapping a
                                                        JOIN employee_master b
                                                        on a.RateeID = b.eid
                                                        where EVId = @EVId", new { EVId });

            return View(reports);

        }






    }
}
