using System;
using System.Collections.Generic;
using System.IO;
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

        var notifications = new List<Notification>();
        foreach (var n in notificationUserStatus)
        {
            var not = _context.Notifications.FirstOrDefault(x => x.UserStatusList.Contains(n) && x.Status > 0);
            if(not!=null)
                notifications.Add(not);
        }

        return notifications;
    }

    [HttpGet]
    [Route("get-all-notifications")]
    public async Task<List<Notification>> GetAllNotifications(string keyName)
    {
        try
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var notificationUserStatus = await _context.NotificationsUserStatus
                .Where(n => n.NotifiedUserId == currentUser.Id && n.Status!=-10)
                .ToListAsync();

            var notifications = new List<Notification>();
            foreach (var n in notificationUserStatus)
            {
                var not = _context.Notifications.FirstOrDefault(x => x.UserStatusList.Contains(n) && x.Status != 0);
                if (not != null)
                    notifications.Add(not);
            }

            var notificationsStarted = await _context.Notifications.AsQueryable()
                .Where(n => n.IdInitiator == currentUser.Id).ToListAsync();

            notifications.AddRange(notificationsStarted);
            return notifications.OrderByDescending(n => n.CreatedAt).ToList();
        }
        catch (Exception)
        {
            return null;
        }
    }


    [HttpGet]
    [Route("get-notification/{id}")]
    public async Task<Notification> GetNotificationById(int id)
    {
        try
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

            return notification;
        }
        catch (Exception)
        {
            return null;
        }
    }

    [HttpPost]
    [Route("accept-notification")]
    public async Task<ActionResult<string>> AcceptNotification(AcceptNotificationPayload payload)
    {
        try
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            if (string.IsNullOrEmpty(payload.SelectedKey))
            {
                return BadRequest("Error: The key was not selected!");
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
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }

    [HttpPost]
    [Route("deny-notification")]
    public async Task<ActionResult<string>> DenyNotification(DenyNotificationPayload payload)
    {
        try
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            if (string.IsNullOrEmpty(payload.RefuseReason))
            {
                return BadRequest("Error: refuse reason can not be null!");
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
            var directoryPath = "/home/razvan/temp_files/notification" + notification.Id;
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            return Ok("SUCCESS");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }

    [HttpGet]
    [Route("get-signature/{notificationId}")]
    public async Task<ActionResult<string>> GetSignature(int notificationId)
    {
        try
        {
            var notification = await _context.Notifications
                .Where(n => n.Id == notificationId).FirstOrDefaultAsync();
            
            if (string.IsNullOrEmpty(notification?.Signature))
                return BadRequest("Signature could not be loaded");

            return await Task.FromResult(notification.Signature);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
    
    [HttpGet]
    [Route("get-public-key/{notificationId}")]
    public async Task<ActionResult<string>> GetPublicKey(int notificationId)
    {
        try
        {
            var notification = await _context.Notifications
                .Where(n => n.Id == notificationId).FirstOrDefaultAsync();
            
            if (string.IsNullOrEmpty(notification?.PublicKey))
                return BadRequest("Signature could not be loaded");

            return await Task.FromResult(notification.PublicKey);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}