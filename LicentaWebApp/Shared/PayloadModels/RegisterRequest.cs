using System.ComponentModel.DataAnnotations;

namespace LicentaWebApp.Shared.PayloadModels;

public class RegisterRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [EmailAddress]
    public string EmailAddress { get; set; }
    public string Password { get; set; }
}