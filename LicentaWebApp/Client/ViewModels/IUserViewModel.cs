using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaWebApp.Client.ViewModels
{
    public interface IUserViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public List<UserViewModel> UserList { get; set; }

        public Task InitializeUserList();
    }
}