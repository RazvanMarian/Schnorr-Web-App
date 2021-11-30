using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace LicentaWebApp.Client.ViewModels
{
    public interface ILoginViewModel
    {
        [EmailAddress]
        public string EmailAddress { get; set; }
        public string Password { get; set; }

        public Task LoginUser();
    }
}