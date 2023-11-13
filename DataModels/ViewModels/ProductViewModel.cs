using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.ViewModels
{
    public class ProductViewModel
    {
        public ProductModel Product { get; set; }
        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }
    }
}
