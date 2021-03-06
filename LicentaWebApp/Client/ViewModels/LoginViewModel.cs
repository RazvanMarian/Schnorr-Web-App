using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Utils;
using LicentaWebApp.Shared.Models;
using LicentaWebApp.Shared.PayloadModels;

namespace LicentaWebApp.Client.ViewModels
{
    public class LoginViewModel: ILoginViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string OtpCode { get; set; }
        public int[] SmartCardCode { get; set; }

        private readonly HttpClient _httpClient;
        public LoginViewModel()
        {

        }
        public LoginViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthenticationResponse> AuthenticateCredentials()
        {
            var authenticationRequest = new AuthenticationRequest
            {
                EmailAddress = this.EmailAddress,
                Password = this.Password
            };
            
            var httpMessageResponse =
                await _httpClient.
                    PostAsJsonAsync($"user/authenticate-credentials", authenticationRequest);
            
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
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
                    PostAsJsonAsync($"user/authenticate-generate-otp", authenticationRequest);
            
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }

        public async Task<AuthenticationResponse> AuthenticateOtp()
        {
            var authenticationRequest = new AuthenticationRequest
            {
                EmailAddress = EmailAddress,
                OtpCode = OtpCode
            };

            var httpMessageResponse =
                await _httpClient.
                    PostAsJsonAsync($"user/authenticate-challenge-otp", authenticationRequest);
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }

        public async Task<AuthenticationResponse> AuthenticateSmartCard(int[] helper)
        {
            var url = "https://192.168.215.67:8443/auth-card";
            
            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, helper);

                var content = await response.Content.ReadAsStringAsync();
                content = content.Trim('[', ']');
                Console.WriteLine(content);

                if (content is "sun.security.smartcardio.PCSCException: SCARD_E_SERVICE_STOPPED" or
                    "sun.security.smartcardio.PCSCException: SCARD_E_NO_READERS_AVAILABLE")
                {
                    return new AuthenticationResponse() { Token = "reader not connected" };
                }

                if (content == "sun.security.smartcardio.PCSCException: SCARD_W_REMOVED_CARD")
                {
                    return new AuthenticationResponse() { Token = "card not connected" };
                }

                var authenticationRequest = new AuthenticationRequest
                {
                    EmailAddress = EmailAddress,
                    SmartCardCode = content.Split(",").Select(Int32.Parse).ToArray()
                };


                var httpMessageResponse =
                    await _httpClient.PostAsJsonAsync($"user/authenticate-challenge-card", authenticationRequest);
                return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
            }
            catch (Exception)
            {
                return new AuthenticationResponse() { Token ="error connecting" };
            }
        }

        public async Task<AuthenticationResponse> GenerateCardCode()
        {
            var authenticationRequest = new AuthenticationRequest
            {
                EmailAddress = this.EmailAddress,
                Password = this.Password
            };
            
            var httpMessageResponse =
                await _httpClient.
                    PostAsJsonAsync($"user/generate-card-code", authenticationRequest);
            
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }

        public async Task<string> RegisterUser()
        {
            var registerRequest = new RegisterRequest
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                EmailAddress = this.EmailAddress,
                Password = this.Password
            };

            var httpMessageResponse = await _httpClient.PostAsJsonAsync("user/register-user",registerRequest);
            return await httpMessageResponse.Content.ReadAsStringAsync();
        }

        public static implicit operator LoginViewModel(User user)
        {
            return new LoginViewModel
            {
                FirstName = user.FirstName,
                LastName=user.LastName,
                EmailAddress = user.EmailAddress,
                Password = user.Password
            };
        }

        public static implicit operator User(LoginViewModel loginViewModel)
        {
            return new User
            {
                FirstName = loginViewModel.FirstName,
                LastName = loginViewModel.LastName,
                EmailAddress = loginViewModel.EmailAddress,
                Password = loginViewModel.Password,
            };
        }
    }
}