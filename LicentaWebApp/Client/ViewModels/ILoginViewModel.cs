using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared;

namespace LicentaWebApp.Client.ViewModels
{
    public interface ILoginViewModel
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        
        
        public Task<AuthenticationResponse> AuthenticateJwt();
    }
}