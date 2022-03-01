using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels;

public class NotificationViewModel :INotificationViewModel
{
    private readonly HttpClient _httpClient;
    public int Id { get; set; }
    public int IdInitiator { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FileContent { get; set; }
    public string FileName { get; set; }
    public List<NotificationUserStatus> UserStatusList { get; set; }
    
    public NotificationViewModel()
    {

    }
    public NotificationViewModel(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<Notification>> GetNotifications()
    {
        var res = await _httpClient.GetFromJsonAsync<List<Notification>>("/notification/get-notifications");
        return res;
    }
    
    public static implicit operator NotificationViewModel(Notification notification)
    {
        return new NotificationViewModel()
        {
            Id = notification.Id,
            IdInitiator = notification.IdInitiator,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            FileContent = notification.FileContent,
            FileName = notification.FileName,
            UserStatusList = notification.UserStatusList
        };
    }

    public static implicit operator Notification(NotificationViewModel notificationViewModel)
    {
        return new Notification()
        {
            Id = notificationViewModel.Id,
            IdInitiator = notificationViewModel.IdInitiator,
            Status = notificationViewModel.Status,
            CreatedAt = notificationViewModel.CreatedAt,
            FileContent = notificationViewModel.FileContent,
            FileName = notificationViewModel.FileName,
            UserStatusList = notificationViewModel.UserStatusList
        };
    }
}