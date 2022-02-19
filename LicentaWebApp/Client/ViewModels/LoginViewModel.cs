using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class LoginViewModel: ILoginViewModel
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        private readonly HttpClient _httpClient;
        public LoginViewModel()
        {

        }
        public LoginViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthenticationResponse> AuthenticateJwt()
        {
            var authenticationRequest = new AuthenticationRequest
            {
                EmailAddress = this.EmailAddress,
                Password = this.Password
            };

            var httpMessageResponse =
                await _httpClient.
                    PostAsJsonAsync<AuthenticationRequest>($"user/authenticate", authenticationRequest);
            
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }
        public static implicit operator LoginViewModel(User user)
        {
            return new LoginViewModel
            {
                EmailAddress = user.EmailAddress,
                Password = user.Password
            };
        }

        public static implicit operator User(LoginViewModel loginViewModel)
        {
            return new User
            {
                FirstName = "first",
                LastName = "last",
                EmailAddress = loginViewModel.EmailAddress,
                Password = loginViewModel.Password,
                DateOfBirth = "fake_date"
            };
        }
    }
}