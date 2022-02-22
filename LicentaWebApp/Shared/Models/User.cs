using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LicentaWebApp.Shared.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(20)]
        public string DateOfBirth { get; set; }

        public int DarkTheme { get; set; }

        public DateTime CreatedDate { get; set; }
        
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }
        
        public List<Key> Keys { get; set; } = new List<Key>();

        [Required]
        public Company Company { get; set; } = new Company();

    }
}
