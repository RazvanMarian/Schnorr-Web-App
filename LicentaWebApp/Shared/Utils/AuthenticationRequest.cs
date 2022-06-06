namespace LicentaWebApp.Shared.Utils
{

    public class AuthenticationRequest
    {
        public string EmailAddress { get; set; }

        public string Password { get; set; }
        
        public string OtpCode { get; set; }
        
        public int[] SmartCardCode { get; set; }
    }
}