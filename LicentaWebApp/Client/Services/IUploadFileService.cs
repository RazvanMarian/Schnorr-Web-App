using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;

namespace LicentaWebApp.Client.Services
{
    public interface IUploadFileService
    {

        public Task<string> UploadHashWithKey(IBrowserFile file, string keyName);

        public Task<HttpResponseMessage> MultipleSignFile(IBrowserFile file, MultipleSignPayload payload);
    }
}
