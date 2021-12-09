using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;

namespace LicentaWebApp.Client.ViewModels
{
    public interface IGenerateKeyViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public Task<HttpResponseMessage> GenerateKey();
    }
}