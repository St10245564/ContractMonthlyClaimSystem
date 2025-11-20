using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Models
{
    public class Module
    {
        [Key]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(20)]
        public string ModuleCode { get; set; }

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal HourlyRate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Claim> Claims { get; set; }
    }
}