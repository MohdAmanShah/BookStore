using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class ShoppingCartModel
    {
        [Key]
        public int CartId { get; set; }
        [Range(1,1000,ErrorMessage = "Enter a number from 1 to 1000")]
        public int Count {  get; set; }
        public int ProductId {  get; set; }
        [ValidateNever]
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

        public string UserId {  get; set; }
        [ValidateNever]
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [NotMapped]
        public double Price { get; set; }

    }
}
