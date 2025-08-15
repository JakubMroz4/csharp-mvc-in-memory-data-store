using System.ComponentModel.DataAnnotations;

namespace exercise.wwwapi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
    }
}
