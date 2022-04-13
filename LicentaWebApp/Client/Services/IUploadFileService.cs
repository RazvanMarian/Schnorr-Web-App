using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;
using LicentaWebApp.Shared.PayloadModels;

namespace LicentaWebApp.Client.Services
{
    public interface IUploadFileService
    {

        public Task<string> SignFile(IBrowserFile file, string keyName, string fileName);

        public Task<HttpResponseMessage> MultipleSignFile(IBrowserFile file, MultipleSignPayload payload);

        public Task<string> VerifyFile(IBrowserFile document, IBrowserFile signatureFile);

        public Task<string> GenerateOtpCode();

        public Task<string> TestOtpCode(string otpCode);
    }
}
