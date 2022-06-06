using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Utils;

namespace LicentaWebApp.Client.ViewModels
{
    public interface ILoginViewModel
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string OtpCode { get; set; }
        public int[] SmartCardCode { get; set; }

        public Task<AuthenticationResponse> AuthenticateCredentials();
        public Task<AuthenticationResponse> AuthenticateJwt();
        public Task<AuthenticationResponse> AuthenticateOtp();
        public Task<AuthenticationResponse> AuthenticateSmartCard(int[] helper);
        public Task<AuthenticationResponse> GenerateCardCode();
    }
}