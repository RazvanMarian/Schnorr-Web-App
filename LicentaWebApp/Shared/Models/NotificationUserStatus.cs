namespace LicentaWebApp.Shared.Models;

public class NotificationUserStatus
{
    public int Id { get; set; }

    public User NotifiedUser { get; set; } = new User();
    
    public int Status { get; set; }
    
    public string SelectedKeyName { get; set; }
    
    public string RefuseReason { get; set; }
}