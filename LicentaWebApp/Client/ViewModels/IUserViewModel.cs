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

        public Task<List<User>> GetCompanyUsers();
        public Task GetUser(int id);
        
    }
}