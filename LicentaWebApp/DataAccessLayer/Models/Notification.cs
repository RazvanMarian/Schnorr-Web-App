using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public class Notification
{
    public int Id { get; set; }
    public int IdInitiator { get; set; }
    public string InitiatorFirstName { get; set; }
    public string InitiatorLastName { get; set; }
    public string InitiatorEmailAddress { get; set; }
    public string SelectedKey { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FilePath{ get; set; }
    public string FileName { get; set; }
    public List<NotificationUserStatus> UserStatusList { get; set; } = new List<NotificationUserStatus>();
}