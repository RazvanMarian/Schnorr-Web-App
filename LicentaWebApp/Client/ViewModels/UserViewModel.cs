using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class UserViewModel
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public static implicit operator UserViewModel(User user)
        {
            return new UserViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }
    }
}