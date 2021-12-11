using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.ViewModels
{
    public class KeyViewModel: IKeyViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Key> Keys { get; set; } = new List<Key>();
        
        private readonly HttpClient _httpClient;
        public KeyViewModel()
        {
            
        }

        public KeyViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<HttpResponseMessage> GenerateKey()
        {
            Console.WriteLine(Name);
            var result = await _httpClient.PostAsJsonAsync<Key>("key/generate", this);
            return result;
        }

        public async Task InitializeKeys()
        {
            Keys = await _httpClient.GetFromJsonAsync<List<Key>>("/key/getKeys");
        }

        public static implicit operator KeyViewModel(Key key)
        {
            return new KeyViewModel
            {
                Name = key.Name,
                Description = key.Description
            };
        }

        public static implicit operator Key(KeyViewModel keyViewModel)
        {
            return new Key
            {
                Name = keyViewModel.Name,
                Description = keyViewModel.Description,
                PublicKeyPath = "Empty",
                PrivateKeyPath = "Empty"
            };
        }
    }
}