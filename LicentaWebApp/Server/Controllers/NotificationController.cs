using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            .Where(n => n.NotifiedUserId == currentUser.Id).ToListAsync();

        List<Notification> notifications = new List<Notification>();
        foreach (var n in notificationUserStatus)
        {
            notifications.Add( _context.Notifications.FirstOrDefault(x => x.UserStatusList.Contains(n)));
            
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
    
}