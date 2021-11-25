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
using DataAccessLayer.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace LicentaWebApp.Server.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly UserContext _context;
        public UploadController(UserContext context)
        {
            _context = context;
        }

        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_sign(string str);

        [HttpPost]
        [Route("uploadHash")]
        public async Task<IActionResult> PostHash(byte[] hashP)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < hashP.Length; i++)
            {
                builder.Append($"{hashP[i]:X2}");
            }
            string hash = builder.ToString();

            test_sign(hash);
            return Ok();
        }
    }
}
