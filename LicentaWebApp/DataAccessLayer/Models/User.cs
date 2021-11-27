using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLayer.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string password { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(20)]
        public string dateOfBirth { get; set; }

        public int darkTheme { get; set; }

        public DateTime createdDate { get; set; }

        [Required]
        public List<Address> Addresses { get; set; } = new List<Address>();

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
