using Microsoft.AspNetCore.Components.Forms;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Utils;
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

        public async Task<string> VerifyFile(IBrowserFile document, IBrowserFile signatureFile)
        {
            var sha256 = SHA256.Create();

            if (document == null || signatureFile == null)
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

            using (var ms = new MemoryStream())
            {
                await signatureFile.OpenReadStream().CopyToAsync(ms);
                payload.SignatureContent = ms.ToArray();
            }
            
            var result =await _httpClient.PostAsJsonAsync("file/verify-file", payload);
            return !result.IsSuccessStatusCode ? null : "Success";
        }

        public async Task<string> GenerateOtpCode()
        {
            var httpMessageResponse =
                await _httpClient.
                    PostAsync($"user/generate-otp-code-auth",null);
            
            return !httpMessageResponse.IsSuccessStatusCode ? "Error" : "Success";
        }

        public async Task<string> TestAuthState()
        {
            var result = await _httpClient.PostAsync("user/test-auth-state",null);
            
            var content = await result.Content.ReadAsStringAsync();
            
            Console.WriteLine(content);
            
            return !result.IsSuccessStatusCode ? "DEAD" : content;
        }

        public async Task<string> ResetTimers()
        {
            var result = await _httpClient.PostAsync("user/reset-timers",null);
            
            var content = await result.Content.ReadAsStringAsync();
            
            return !result.IsSuccessStatusCode ? null : content;
        }

        public async Task<string> TestOtpCode(string otpCode)
        {
            var result = await _httpClient.PostAsJsonAsync("user/test-otp", otpCode);
            if (!result.IsSuccessStatusCode)
                return null;
            
            
            return "Success!";
        }
        
        public async Task<AuthenticationResponse> GenerateCardCode()
        {
            var httpMessageResponse =
                await _httpClient.
                    PostAsync($"user/generate-card-code-auth",null);
            
            return await httpMessageResponse.Content.ReadFromJsonAsync<AuthenticationResponse>();
        }
        
        public async Task<string> AuthenticateSmartCard(int[] helper)
        {
            var url = "https://192.168.192.67:8443/auth-card";
            
            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;
            
            var response = await _httpClient.PostAsJsonAsync(url,helper);
            
            var content = await response.Content.ReadAsStringAsync();
            content = content.Trim('[', ']');
            Console.WriteLine(content);

            if (content == "sun.security.smartcardio.PCSCException: SCARD_E_SERVICE_STOPPED")
            {
                return "reader not connected";
            }

            if (content == "sun.security.smartcardio.PCSCException: SCARD_W_REMOVED_CARD")
            {
                return "card not connected";
            }
            
            var authenticationRequest = new AuthenticationRequest
            {
                SmartCardCode = content.Split(",").Select(Int32.Parse).ToArray()
            };
            

            var httpMessageResponse =
                await _httpClient.
                    PostAsJsonAsync($"user/authenticate-challenge-card-auth", authenticationRequest);
            if (!httpMessageResponse.IsSuccessStatusCode)
                return null;
            
            return "Success!";
        }
    }
}