using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContractMonthlyClaimSystem.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select user type")]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        public List<SelectListItem> UserTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Lecturer", Text = "Lecturer" },
            new SelectListItem { Value = "Coordinator", Text = "Programme Coordinator" },
            new SelectListItem { Value = "Manager", Text = "Academic Manager" }
        };
    }
}