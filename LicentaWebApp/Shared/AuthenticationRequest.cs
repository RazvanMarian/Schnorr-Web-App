namespace LicentaWebApp.Shared
{

    public class AuthenticationRequest
    {
        public string EmailAddress { get; set; }

        public string Password { get; set; }
        
        public string OtpCode { get; set; }
    }
}