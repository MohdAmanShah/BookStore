using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.ViewModels
{
    public class UserPermissionViewModel
    {
        public ApplicationUser User { get; set; }
        public List<String?> Roles { get; set; }
        public List<CompanyModel>? Companies { get; set; }

    }
}
