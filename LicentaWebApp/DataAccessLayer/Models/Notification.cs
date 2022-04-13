using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DataAccessLayer.Models;

public class Notification
{
    public int Id { get; set; }
    [Required]
    public int IdInitiator { get; set; }
    public string InitiatorFirstName { get; set; }
    public string InitiatorLastName { get; set; }
    public string InitiatorEmailAddress { get; set; }
    public string SelectedKey { get; set; }
    [Required]
    public int Status { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    public string FilePath{ get; set; }
    [Required]
    public string FileName { get; set; }
    public string Signature { get; set; }
    public string PublicKey { get; set; }
    [AllowNull]
    public List<NotificationUserStatus> UserStatusList { get; set; } = new List<NotificationUserStatus>();
}