using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
namespace ContractMonthlyClaimSystem.Models
{
    public class CreateClaimViewModel
    {
        [Required(ErrorMessage = "Module code is required")]
        [StringLength(20, ErrorMessage = "Module code cannot exceed 20 characters")]
        [Display(Name = "Module Code")]
        public string ModuleCode { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0.01, 1000, ErrorMessage = "Hourly rate must be between 0.01 and 1000")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Claim Date")]
        public DateTime ClaimDate { get; set; } = DateTime.Today;

        [Required]
        [Range(0.5, 100, ErrorMessage = "Hours worked must be between 0.5 and 100")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [StringLength(500)]
        [Display(Name = "Description of Work")]
        public string Description { get; set; }
    }
}