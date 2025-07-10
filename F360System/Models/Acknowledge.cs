using System.Collections;

namespace F360System.Models
{
    public class Acknowledge
    {
        public int Id { get; set; }
        public int EVId { get; set; }
        public string EId { get; set; }
        public string EmpName { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime Deadline { get; set; }

        public string AcknowledgeBy { get; set; }
        public DateTime AcknowledgeDate { get; set; }
        public string AcknowledgeRemarks { get; set; }
        public string AcknowledgeStatus { get; set; }

        public string ConfirmRemarks { get; set; }






        public IEnumerable<Acknowledge> acknowledge { get; set; }


        public IEnumerable<PrintRate> printRate { get; set; }
        public IEnumerable<PrintRemarks> printRemarks { get; set; }
        public List<PrintRate> GroupedRatings { get; set; }
        public PrintRate Overall { get; set; }
        public Confirm Confirm { get; set; }
        public List<GroupedRemark> GroupedRemarks { get; set; }
    }


    public class GroupedRemark
    {
        public string Category { get; set; }
        public List<RelationGroup> SubGroups { get; set; }
    }

    public class RelationGroup
    {
        public string Relation { get; set; }
        public List<PrintRemarks> Remarks { get; set; }
    }





    public class PrintRate
    {
        public string FormName { get; set; }
        public string Name { get; set; }
        public string DepName { get; set; }
        public string Category { get; set; }
        public int Seq { get; set; }
        public string Questionaire { get; set; }
        public decimal Head { get; set; }
        public decimal Peers { get; set; }
        public decimal InternalCustomer { get; set; }
        public decimal DirectReport { get; set; }
        public decimal Self { get; set; }
        public decimal AvgAll { get; set; }
        public decimal AvgNS { get; set; }

        // Used when this acts as a "group" container
        public List<PrintRate> Items { get; set; }

        // Group-level averages (used if this is a category)
        public double AvgHead { get; set; }
        public double AvgPeers { get; set; }
        public double AvgCustomer { get; set; }
        public double AvgDR { get; set; }
        public double AvgSelf { get; set; }
        public double AvgAllGroup { get; set; }
        public double AvgNoSelfGroup { get; set; }

    }



    public class PrintRemarks
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string Category { get; set; }
        public string Relation { get; set; }
        public string Remarks { get; set; }
    }



    public class Confirm 
    {
        public string UserId { get; set; }
        public string password{ get; set; }

        public string ConfirmRemarks { get; set; }
    }
}
