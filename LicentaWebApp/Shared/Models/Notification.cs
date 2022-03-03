using System;
using System.Collections.Generic;

namespace LicentaWebApp.Shared.Models;

public class Notification
{
    public int Id { get; set; }

    public int IdInitiator { get; set; }

    public string SelectedKey { get; set; }
    public int Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string FilePath { get; set; }
    
    public string FileName { get; set; }

    public List<NotificationUserStatus> UserStatusList { get; set; } = new List<NotificationUserStatus>();
}