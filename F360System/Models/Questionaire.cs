namespace F360System.Models
{
    public class Questionaire
    {


        public int HId { get; set; }
        public string FormName { get; set; }
        public string Remarks { get; set; }
        public IEnumerable<QuestionaireDetails> QuestionDetails { get; set; }
        public IEnumerable<Questionaire> QuestionHeaders { get; set; }

        public QuestionaireDetails QuestionDetail { get; set; }
        public string[] categories { get; set; }


    }

    public class QuestionaireDetails 
    {
        public int DId { get; set; }
        public int HId { get; set; }
        public int CId { get; set; }
        public string CategoryName { get; set; }
        public string Category { get; set; }
        public int Seq { get; set; }
        public string Questionaire { get; set; }
        
    }
}
