using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels;

public class NotificationViewModel : INotificationViewModel
{
    private readonly HttpClient _httpClient;
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

    public async Task<Notification> GetNotificationById(int id)
    {
        var result = await _httpClient.GetFromJsonAsync<Notification>($"/notification/get-notification/{id}");
        return result;
    }

    public async Task<HttpResponseMessage> DenyNotification(int notificationId, string refuseReason)
    {
        var payload = new DenyNotificationPayload
        {
            NotificationId = notificationId,
            RefuseReason = refuseReason
        };
        var result = await _httpClient
            .PostAsJsonAsync("notification/deny-notification", payload);
        return result;
    }

    public async Task<HttpResponseMessage> AcceptNotification(int notificationId, string selectedKey)
    {
        var payload = new AcceptNotificationPayload
        {
            NotificationId = notificationId,
            SelectedKey = selectedKey
        };

        var result = await _httpClient
            .PostAsJsonAsync("notification/accept-notification", payload);
        return result;
    }

    public async Task<List<Notification>> GetAllNotifications()
    {
        var res = await _httpClient.GetFromJsonAsync<List<Notification>>("/notification/get-all-notifications");
        return res;
    }

    public async Task<string> MultipleSignFinish(int notificationId)
    {
        
        var result = await _httpClient.PostAsJsonAsync($"upload/multiple-sign",notificationId);
        return !result.IsSuccessStatusCode ? null : result.Content.ReadAsStringAsync().Result;
    }

    public static implicit operator NotificationViewModel(Notification notification)
    {
        return new NotificationViewModel
        {
            Id = notification.Id,
            IdInitiator = notification.IdInitiator,
            InitiatorFirstName = notification.InitiatorFirstName,
            InitiatorLastName = notification.InitiatorLastName,
            InitiatorEmailAddress = notification.InitiatorEmailAddress,
            SelectedKey = notification.SelectedKey,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            FilePath = notification.FilePath,
            FileName = notification.FileName,
            UserStatusList = notification.UserStatusList
        };
    }

    public static implicit operator Notification(NotificationViewModel notificationViewModel)
    {
        return new Notification
        {
            Id = notificationViewModel.Id,
            IdInitiator = notificationViewModel.IdInitiator,
            InitiatorFirstName = notificationViewModel.InitiatorFirstName,
            InitiatorLastName = notificationViewModel.InitiatorLastName,
            InitiatorEmailAddress = notificationViewModel.InitiatorEmailAddress,
            SelectedKey = notificationViewModel.SelectedKey,
            Status = notificationViewModel.Status,
            CreatedAt = notificationViewModel.CreatedAt,
            FilePath = notificationViewModel.FilePath,
            FileName = notificationViewModel.FileName,
            UserStatusList = notificationViewModel.UserStatusList
        };
    }
}