using System.ComponentModel.DataAnnotations;

namespace MsAuth.Application.Models
{
    public sealed record RegisterRequest(
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        string Email, 
        
        [Required(ErrorMessage = "Name is required")]
        [MinLength(1, ErrorMessage = "Name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        string Name, 
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        string Password
    );
}