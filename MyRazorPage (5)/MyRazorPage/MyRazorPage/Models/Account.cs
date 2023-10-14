using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyRazorPage.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        public string? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public int? Role { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpireAt { get; set; }
        public bool? Status { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
