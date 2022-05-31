using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared;
using LicentaWebApp.Shared.Models;
using LicentaWebApp.Shared.PayloadModels;

namespace LicentaWebApp.Client.Services
{
    public interface IUploadFileService
    {

        public Task<string> SignFile(IBrowserFile file, string keyName, string fileName);
        public Task<HttpResponseMessage> MultipleSignFile(IBrowserFile file, MultipleSignPayload payload);
        public Task<string> VerifyFile(IBrowserFile document, IBrowserFile signatureFile);
        public Task<AuthenticationResponse> GenerateCardCode();
        public Task<string> GenerateOtpCode();
        public Task<string> TestAuthState();
        public Task<string> ResetTimers();
        public Task<string> AuthenticateSmartCard(int[] helper);
        public Task<string> TestOtpCode(string otpCode);
    }
}
