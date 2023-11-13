using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModels
{
    public class ProductImagesModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Url { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        [ValidateNever]
        public  ProductModel Product { get; set; }  

    }
}