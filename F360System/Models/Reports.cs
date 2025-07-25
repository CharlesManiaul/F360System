namespace F360System.Models
{
    public class Reports
    {
        public int Id { get; set; }
        public int EVId { get; set; }
        public string EId { get; set; }
        public string EmpName { get; set; }
        public string RateeId { get; set; }
        public DateTime AnswerDate { get; set; }
        public string OverRemarks { get; set; }

        public string FormName { get; set; }
        public int DId { get; set; }
        public int Rate { get; set; }

        public decimal Self { get; set; }
        public decimal Head { get; set; }
        public decimal Peers { get; set; }
        public decimal DirectReport { get; set; }
        public decimal InternalCustomer { get; set; }

        public decimal Overall { get; set; }

        public string RateeName { get; set; }
        public string relation { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime Deadline { get; set; }

        public string AcknowledgeBy { get; set; }
        public DateTime AcknowledgeDate { get; set; }
        public string AcknowledgeRemarks { get; set; }
        public string AcknowledgeStatus { get; set; }

        public IEnumerable<Reports> reports { get; set; }

        public IEnumerable<MapDetails> mapDetails { get; set; }

        public IEnumerable<Mapping> map { get; set; }

    }
}
