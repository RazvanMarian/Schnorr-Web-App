using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class UserViewModel:IUserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Company Company { get; set; }

        private readonly HttpClient _httpClient;


        public UserViewModel()
        {
            
        }
        public UserViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<List<User>> GetCompanyUsers()
        {
            var companyUserList = _httpClient
                .GetFromJsonAsync<List<User>>("/user/getcompanyusers");

            return companyUserList;
        }

        public async Task GetUser(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<User>("/user/getuser/" + id);
            LoadCurrentObject(user);
        }

        private void LoadCurrentObject(UserViewModel user)
        {
            this.Id = user.Id;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.Company = user.Company;
        }
        
        public static implicit operator UserViewModel(User user)
        {
            return new UserViewModel
            {
                Id=user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Company = user.Company
            };
        }

        public static implicit operator User(UserViewModel user)
        {
            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Company = user.Company
            };
        }
    }
}