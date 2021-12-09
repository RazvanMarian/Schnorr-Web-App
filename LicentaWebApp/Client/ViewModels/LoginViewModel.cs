using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

        public async Task LoginUser()
        {
            await _httpClient.PostAsJsonAsync<User>("user/loginuser", this);
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