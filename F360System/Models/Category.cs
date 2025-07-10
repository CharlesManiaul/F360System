namespace F360System.Models
{
    public class Category
    {
        public int CId { get; set; }
        public decimal Seq { get; set; }
        public string CategoryName { get; set; }
        public string category { get; set; }
        public IEnumerable<Category> categories { get; set; }

    }




}
