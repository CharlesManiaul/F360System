namespace F360System.Models
{
    public class EvalResult
    {
        public int Id { get; set; }
        public int EVId { get; set; }
        public string EmpName { get; set; }
        public string RateeId { get; set; }
        public DateTime UpdtDate { get; set; }
        public string OverRemarks { get; set; }

        public List<EvalRating> evalRatings { get; set; }
        public List<EvalRemarks> evalRemarks { get; set; }

    }

    public class EvalRating 
    {
        public int Id { get; set; }
        public int ERId { get; set; }
        public int DId { get; set; }
        public int Rate { get; set; }
        public string Category { get; set; }
        public int Seq { get; set; }
        public string Questionaire { get; set; }
        public string Remarks { get; set; }


    }

    public class EvalRemarks 
    {
        public int Id { get; set; }
        public int ERId { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }
    }
}
