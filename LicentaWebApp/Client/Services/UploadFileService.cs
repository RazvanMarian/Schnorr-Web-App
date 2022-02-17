﻿using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
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
            
            var sha256 = SHA256.Create();
            
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
                
                await _httpClient.PostAsJsonAsync("upload/uploadHash", hash);
            }

        }

        public async Task<string> UploadHashWithKey(IBrowserFile file, string keyName)
        {
            var sha256 = SHA256.Create();
            
            if (file == null)
            {
                return null;
            }
            var ms = new MemoryStream();
            await file.OpenReadStream().CopyToAsync(ms);
            var buffer = ms.ToArray();
            var hash = sha256.ComputeHash(buffer);
            var hashString =  BitConverter.ToString(hash).Replace("-", "").ToLower();
            
            var result =
                await _httpClient.PostAsJsonAsync("upload/sign/file/" +  hashString, keyName);
            
            return !result.IsSuccessStatusCode ? null : result.Content.ReadAsStringAsync().Result;
        }
    }
}
