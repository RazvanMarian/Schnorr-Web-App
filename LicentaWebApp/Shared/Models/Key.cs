using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicentaWebApp.Shared.Models
{
    public class Key
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(256)]
        public string Description { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string PublicKeyPath { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string PrivateKeyPath { get; set; }
        
        [DefaultValue("active")]
        public string State { get; set; }
        
        [ForeignKey("PK_Keys")]
        public int UserId { get; set; }
    }
}