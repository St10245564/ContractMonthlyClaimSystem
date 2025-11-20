using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContractMonthlyClaimSystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please select user type")]
        [Display(Name = "User Type")]
        public string UserType { get; set; }

        public List<SelectListItem> UserTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Lecturer", Text = "Lecturer" },
            new SelectListItem { Value = "Coordinator", Text = "Programme Coordinator" },
            new SelectListItem { Value = "Manager", Text = "Academic Manager" }
        };
    }
}