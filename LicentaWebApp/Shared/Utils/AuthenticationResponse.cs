namespace LicentaWebApp.Shared.Utils
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public int[] helperCode { get; set; }
    }
}