using System.Collections.Generic;

namespace LicentaWebApp.Shared.Models;

public class MultipleSignPayload
{
    public List<User> Users { get; set; } = new List<User>();
    
    public string FileHash { get; set; }
    
    public string FileName { get; set; }
    
    public string FileContent { get; set; }
    
    public string UserKeyName { get; set; }
}