using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(128)]
        public string Password { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string OtpCode { get; set; }
        public DateTime OtpCreationTime { get; set; }
        public DateTime CardCodeCreationTime { get; set; }
        public List<Key> Keys { get; set; } = new List<Key>();
        [Required]
        public Company Company { get; set; } = new Company();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
