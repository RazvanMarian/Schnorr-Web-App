using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models;

public class Company
{
    public int Id { get; set; }
        
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
        
    [MaxLength(256)]
    public string Description { get; set; }
    
    [MaxLength(128)]
    public string Address { get; set; }
    
}