using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LicentaWebApp.Client.Services
{
    public interface IUploadFileService
    {
        Task UploadHashFile(IBrowserFile file);

        public Task<string> UploadHashWithKey(IBrowserFile file, string keyName);
    }
}
