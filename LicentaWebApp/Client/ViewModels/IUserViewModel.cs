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
        public string EmailAddress { get; set; }
        public bool ValidOtp { get; set; }
        public bool ValidSmartCard { get; set; }
        public int[] SmartCardCode { get; set; }


    }
}