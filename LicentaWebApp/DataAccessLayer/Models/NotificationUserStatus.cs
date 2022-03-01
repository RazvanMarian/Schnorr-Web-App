namespace DataAccessLayer.Models;

public class NotificationUserStatus
{
    public int Id { get; set; }

    public int NotifiedUserId { get; set; }
    
    public int Status { get; set; }
    
    public string SelectedKeyName { get; set; }
}