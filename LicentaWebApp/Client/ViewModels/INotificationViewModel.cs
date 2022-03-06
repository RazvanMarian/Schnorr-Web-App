using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels;

public interface INotificationViewModel
{
    public int Id { get; set; }
    public int IdInitiator { get; set; }
    public string InitiatorFirstName { get; set; }
    public string InitiatorLastName { get; set; }
    public string InitiatorEmailAddress { get; set; }
    public string SelectedKey { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public List<NotificationUserStatus> UserStatusList { get; set; }
    public Task<List<Notification>> GetNotifications();
    public Task<Notification> GetNotificationById(int id);
}