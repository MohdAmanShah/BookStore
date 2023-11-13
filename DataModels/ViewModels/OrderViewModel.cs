using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.ViewModels
{
    public class OrderViewModel
    {
        public OrderHeader orderHeader { get; set; }
        [ValidateNever]
        public IEnumerable<OrderDetail> orderDetails { get; set; }
    }
}
