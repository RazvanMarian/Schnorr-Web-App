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
        [MaxLength(50)]
        public string FirstName { get; set; }


        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(20)]
        public string Age { get; set; }

        [Required]
        public List<Address> Addresses { get; set; } = new List<Address>();

        [Required]
        public List<Email> EmailAddresses { get; set; } = new List<Email>();
    }
}
