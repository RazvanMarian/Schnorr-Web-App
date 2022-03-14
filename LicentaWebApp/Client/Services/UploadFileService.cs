using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;
using LicentaWebApp.Shared.PayloadModels;

namespace LicentaWebApp.Client.Services
{
    public class UploadFileService : IUploadFileService
    {
        private readonly HttpClient _httpClient;

        public UploadFileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<string> SignFile(IBrowserFile file, string keyName, string fileName)
        {
            var sha256 = SHA256.Create();

            if (file == null)
            {
                return null;
            }

            const long maxFileSize = 1024 * 1024 * 1024;
            var ms = new MemoryStream();
            await file.OpenReadStream(maxFileSize).CopyToAsync(ms);
            var buffer = ms.ToArray();
            byte[] pdf = {0x25, 0x50, 0x44, 0x46};

            if (!buffer.Take(4).SequenceEqual(pdf))
                return null;

            var hash = sha256.ComputeHash(buffer);
            var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();

            SignPayload payload = new()
            {
                Hash = hashString,
                KeyName = keyName,
                FileName = fileName
            };

            var result =
                await _httpClient.PostAsJsonAsync("file/sign", payload);

            return !result.IsSuccessStatusCode ? null : result.Content.ReadAsStringAsync().Result;
        }

        public async Task<HttpResponseMessage> MultipleSignFile(IBrowserFile file, MultipleSignPayload payload)
        {
            if (file == null)
            {
                return null;
            }

            var ms = new MemoryStream();
            await file.OpenReadStream(file.Size).CopyToAsync(ms);
            ms.Close();

            payload.FileContent = ms.ToArray();
            byte[] pdf = {0x25, 0x50, 0x44, 0x46};
            if (!payload.FileContent.Take(4).SequenceEqual(pdf))
                return null;

            var res = await _httpClient.PostAsJsonAsync("file/multiple-sign-request", payload);
            return res;
        }

        public async Task<string> VerifyFile(IBrowserFile document, IBrowserFile signatureFile,
            IBrowserFile publicKeyFile)
        {
            var sha256 = SHA256.Create();

            if (document == null || signatureFile == null || publicKeyFile == null)
            {
                return null;
            }

            const long megaBytes = 1024 * 1024;
            var payload = new VerifyFilePayload();
            using (var ms = new MemoryStream())
            {
                await document.OpenReadStream(15 * megaBytes).CopyToAsync(ms);
                var buffer = ms.ToArray();

                byte[] pdf = {0x25, 0x50, 0x44, 0x46};
                if (!buffer.Take(4).SequenceEqual(pdf))
                    return null;

                payload.FileHash = BitConverter
                    .ToString(sha256.ComputeHash(buffer))
                    .Replace("-", "").ToLower();
            }

            if (signatureFile.Size != 64)
                return null;
            
            using (var ms = new MemoryStream())
            {
                await signatureFile.OpenReadStream(64).CopyToAsync(ms);
                payload.SignatureContent = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                await publicKeyFile.OpenReadStream(5 * megaBytes).CopyToAsync(ms);
                payload.CertificateContent = ms.ToArray();
            }

            var result =await _httpClient.PostAsJsonAsync("file/verify-file", payload);
            return !result.IsSuccessStatusCode ? null : "Success";
        }
    }
}