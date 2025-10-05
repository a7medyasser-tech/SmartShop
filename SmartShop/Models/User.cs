using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace SmartShop.Models
{
        public class User
        {
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Password { get; set; }

            public string PhoneNumber { get; set; }
            public string Address { get; set; }

            public string Role { get; set; } 

            public ICollection<Order> Orders { get; set; }
            public ICollection<Cart> Carts { get; set; }
            public bool IsAdmin() => Role == "Admin";

    }
}

