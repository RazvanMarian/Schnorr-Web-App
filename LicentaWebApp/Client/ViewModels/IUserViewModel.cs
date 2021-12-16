using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public interface IUserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public List<UserViewModel> UserList { get; set; }

        public Task InitializeUserList();

        public Task GetUser(int id);
        
    }
}