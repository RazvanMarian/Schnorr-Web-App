using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public interface IKeyViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Key> Keys { get; set; }
        
        public Task<HttpResponseMessage> GenerateKey();
        public Task InitializeKeys();

        public Task<HttpResponseMessage> DeleteKey(Key k);

        public Task<HttpResponseMessage> RenameKey(Key k);

        public Task<string> GenerateCertificate(Key k);
        public Task<List<User>> GetCompanyUsers();
    }
}