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
            var result = await _httpClient.PostAsJsonAsync<Key>("key/generate", this);
            return result;
        }

        public async Task InitializeKeys()
        {
            Keys = await _httpClient.GetFromJsonAsync<List<Key>>("/key/getKeys");
        }

        public async Task<HttpResponseMessage> DeleteKey(Key k)
        {
            var result = await _httpClient.DeleteAsync("key/deleteKey/" + k.Name);
            return result;
        }

        public async Task<HttpResponseMessage> RenameKey(Key k)
        {
            var result = await _httpClient.PostAsJsonAsync("key/renameKey", k);
            return result;
        }

        public async Task<string> GenerateCertificate(Key k)
        {
            var result = await _httpClient.GetAsync("cert/create-cert/" + k.Name);
            var base64 = result.Content.ReadAsStringAsync().Result;
            return base64;
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