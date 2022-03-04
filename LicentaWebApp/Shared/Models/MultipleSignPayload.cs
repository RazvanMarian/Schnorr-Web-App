using System.Collections.Generic;

namespace LicentaWebApp.Shared.Models;

public class MultipleSignPayload
{
    public List<User> Users { get; set; } = new List<User>();
    
    public string FileName { get; set; }
    
    public byte[] FileContent { get; set; }
    
    public string UserKeyName { get; set; }
    
}