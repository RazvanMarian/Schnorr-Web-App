using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared.PayloadModels;
using LicentaWebApp.Shared.Utils;

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
