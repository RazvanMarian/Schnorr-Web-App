using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace LicentaWebApp.Server.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        
        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_sign(string str);

        [HttpPost]
        [Route("uploadHash")]
        public async Task<IActionResult> PostHash(byte[] hashP)
        {
            var hash =  BitConverter.ToString(hashP).Replace("-", "").ToLower();

            test_sign(hash);
            await Task.Delay(2000);
            return Ok();
        }
    }
}
