using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class UserViewModel:IUserViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<UserViewModel> UserList { get; set; } = new List<UserViewModel>();
       

        private readonly HttpClient _httpClient;


        public UserViewModel()
        {
            
        }
        public UserViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task InitializeUserList()
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>("user/getusers");

            if (users != null)
            {
                UserViewModel us = new UserViewModel();
                foreach (var u in users)
                {
                    us = u;
                    UserList.Add(us);
                }
                Console.WriteLine(UserList.FirstOrDefault().FirstName);
            }
        }
        
        public static implicit operator UserViewModel(User user)
        {
            return new UserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public static implicit operator User(UserViewModel user)
        {
            return new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}