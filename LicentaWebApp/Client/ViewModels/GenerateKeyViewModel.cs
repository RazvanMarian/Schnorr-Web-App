using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class GenerateKeyViewModel: IGenerateKeyViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        
        private readonly HttpClient _httpClient;
        public GenerateKeyViewModel()
        {
            
        }

        public GenerateKeyViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<HttpResponseMessage> GenerateKey()
        {
            Console.WriteLine(Name);
            var result = await _httpClient.PostAsJsonAsync<Key>("key/generate", this);
            return result;
        }
        
        public static implicit operator GenerateKeyViewModel(Key key)
        {
            return new GenerateKeyViewModel
            {
                Name = key.Name,
                Description = key.Description
            };
        }

        public static implicit operator Key(GenerateKeyViewModel generateKeyViewModel)
        {
            return new Key
            {
                Name = generateKeyViewModel.Name,
                Description = generateKeyViewModel.Description,
                PublicKeyPath = "Empty",
                PrivateKeyPath = "Empty"
            };
        }
    }
}