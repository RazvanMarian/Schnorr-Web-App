using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace LicentaWebApp.Server.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Hello(string str);

        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void test_sign(string str);

        private readonly IWebHostEnvironment environment;
        public UploadController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }


        [HttpPost]
        [Route("uploadFile")]
        public async Task Post()
        {

            if (HttpContext.Request.Form.Files.Any())
            {
                foreach (var file in HttpContext.Request.Form.Files)
                {
                    var path = Path.Combine(environment.ContentRootPath, "upload", file.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

        }

        [HttpPost]
        [Route("uploadHash")]
        public async Task<IActionResult> PostHash(byte[] hash)
        {

            var builder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append($"{hash[i]:X2}");
            }
            string _hash = builder.ToString();
            test_sign(_hash);
            return Ok();
        }
    }
}
