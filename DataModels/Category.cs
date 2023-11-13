using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

    }
}