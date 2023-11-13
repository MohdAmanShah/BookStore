using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class CompanyModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public String? Street { get; set; }
        public String? Postal { get; set; }
        public String? City { get; set; }
        public String? State { get; set; }
        public String? PhoneNumber { get; set; }

    }
}
