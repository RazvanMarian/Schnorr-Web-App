using System.Collections.Generic;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Shared.PayloadModels;

public class MultipleSignPayload
{
    public List<User> Users { get; set; } = new List<User>();
    
    public string FileName { get; set; }
    
    public byte[] FileContent { get; set; }
    
    public string UserKeyName { get; set; }
    
}