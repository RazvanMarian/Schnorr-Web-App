using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using LicentaWebApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification = DataAccessLayer.Models.Notification;
using User = DataAccessLayer.Models.User;

namespace LicentaWebApp.Server.Controllers;

[Route("notification")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly UserContext _context;

    public NotificationController(UserContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("get-notifications")]
    public async Task<List<Notification>> GetNotifications(string keyName)
    {
        var currentUser = new User();
        if (User.Identity is {IsAuthenticated: true})
        {
            currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
            currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        var notificationUserStatus = await _context.NotificationsUserStatus
            .Where(n => n.NotifiedUserId == currentUser.Id && n.Status == 2)
            .ToListAsync();

        List<Notification> notifications = new List<Notification>();
        foreach (var n in notificationUserStatus)
        {
            notifications.Add(_context.Notifications.FirstOrDefault(x => x.UserStatusList.Contains(n)));
        }

        return notifications;
    }

    [HttpGet]
    [Route("get-notification/{id}")]
    public async Task<Notification> GetNotificationById(int id)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        return notification;
    }

    [HttpPost]
    [Route("accept-notification")]
    public async Task<ActionResult<string>> AcceptNotification(AcceptNotificationPayload payload)
    {
        var currentUser = new User();
        if (User.Identity is {IsAuthenticated: true})
        {
            currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
            currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        
        var notification = await _context
            .Notifications
            .Include(n => n.UserStatusList)
            .FirstOrDefaultAsync(n => n.Id == payload.NotificationId);
        if (notification == null)
        {
            return BadRequest("Error");
        }

        var notificationUserStatus = notification
            .UserStatusList
            .Find(n => n.NotifiedUserId == currentUser.Id);

        if (notificationUserStatus == null)
        {
            return BadRequest("Error");
        }
            

        notificationUserStatus.Status = 0;
        notificationUserStatus.SelectedKeyName = payload.SelectedKey;
        notification.Status--;

        await _context.SaveChangesAsync();

        return Ok("SUCCESS");
    }
    
    [HttpPost]
    [Route("deny-notification")]
    public async Task<ActionResult<string>> DenyNotification(DenyNotificationPayload payload)
    {
        var currentUser = new User();
        if (User.Identity is {IsAuthenticated: true})
        {
            currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
            currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        var notification = await _context
            .Notifications
            .Include(n => n.UserStatusList)
            .FirstOrDefaultAsync(n => n.Id == payload.NotificationId);

        if (notification == null)
        {
            return BadRequest("Error");
        }

        var notificationUserStatus = notification
            .UserStatusList
            .Find(n => n.NotifiedUserId == currentUser.Id);

        if (notificationUserStatus == null)
        {
            return BadRequest("Error");
        }
            

        notificationUserStatus.Status = -1;
        notificationUserStatus.RefuseReason = payload.RefuseReason;
        notification.Status = -2;

        await _context.SaveChangesAsync();
        
        return Ok("SUCCESS");
    }
}