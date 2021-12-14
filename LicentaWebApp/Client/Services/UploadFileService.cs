using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LicentaWebApp.Client.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly HttpClient _httpClient;

        public UploadFileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task UploadHashFile(IBrowserFile file)
        {
            
            SHA256 sha256 = SHA256.Create();
            
            if (file != null)
            {
                var ms = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(ms);
                var buffer = ms.ToArray();
                var hash = sha256.ComputeHash(buffer);
                for (int i = 0; i < hash.Length; i++)
                {
                    Console.Write($"{hash[i]:X2}");
                    if ((i % 4) == 3) Console.Write(" ");
                }
                Console.WriteLine(hash);
                Console.WriteLine("ar fi trebuit ca mai sus sa fie hash-ul");
                await _httpClient.PostAsJsonAsync("upload/uploadHash", hash);
            }

        }

        public async Task UploadHashWithKey(IBrowserFile file, string keyName)
        {
            SHA256 sha256 = SHA256.Create();
            
            if (file != null)
            {
                var ms = new MemoryStream();
                await file.OpenReadStream().CopyToAsync(ms);
                var buffer = ms.ToArray();
                var hash = sha256.ComputeHash(buffer);
                for (int i = 0; i < hash.Length; i++)
                {
                    Console.Write($"{hash[i]:X2}");
                    if ((i % 4) == 3) Console.Write(" ");
                }
                var hashString =  BitConverter.ToString(hash).Replace("-", "").ToLower();
                await _httpClient.PostAsJsonAsync("upload/sign/file/" +  hashString, keyName);
            }
        }
    }
}
