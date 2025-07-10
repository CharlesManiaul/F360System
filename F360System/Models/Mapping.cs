using System.Collections;

namespace F360System.Models
{
    public class Mapping
    {
        public int EVId { get; set; }
        public int HId { get; set; }
        public string EId { get; set; }


        public string EmpName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public DateTime DateFrom { get; set; } = DateTime.Today;
        public DateTime DateTo { get; set; } = DateTime.Today;
        public DateTime DeadLine { get; set; } = DateTime.Today;

        public string Status { get; set; }
        public string AcknowledgeBy { get; set; }
        public DateTime AcknowledgeDate { get; set; }

        public string AcknowledgeName { get; set; }


        public IEnumerable<Mapping> map { get; set; }
        public IEnumerable<EmpMaster> EmpList { get; set; }

        public IEnumerable<EmpMaster> HeadList { get; set; }


        public IEnumerable<MapDetails> MapDetails { get; set; }
    
        public MapDetails mapDetails { get; set; }

        public IEnumerable<Questionaire> FormHeader { get; set; }

        public IEnumerable<SubordinateView> subordinate { get; set; }



    }

    public class EmpMaster
    {
        public string EId { get; set; }
        public string EmpName { get; set; }

    }


    public class SubordinateView
    {
        public string EId { get; set; }
        public string EmpName { get; set; }

        public string sub { get; set; }

    }

    public class MapDetails
    {
        public string Relation { get; set; }
        public string Ratee { get; set; }
        public string RateeName { get; set; }
        public string RateeId { get; set; }



    }






}
