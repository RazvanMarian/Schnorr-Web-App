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
    [Route("[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        
        //[DllImport("../../x64/Debug/SchnorrSig.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void Hello();
        private readonly IWebHostEnvironment environment;
        public UploadController(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }


        //[HttpPost]
        //public async Task Post()
        //{

        //    if (HttpContext.Request.Form.Files.Any())
        //    {
        //        foreach (var file in HttpContext.Request.Form.Files)
        //        {
        //            var path = Path.Combine(environment.ContentRootPath, "upload", file.FileName);

        //            using (var stream = new FileStream(path, FileMode.Create))
        //            {
        //                await file.CopyToAsync(stream);
        //            }
        //        }
        //    }

        //    //Hello();
        //}

        [HttpPost]
        public async Task<IActionResult> PostHash(byte[] hash)
        {
            for (int i = 0; i < hash.Length; i++)
            {
                Console.Write($"{hash[i]:X2}");
                if ((i % 4) == 3) Console.Write(" ");
            }
            Console.WriteLine();
            Console.WriteLine("Am ajuns aici salutare");
            return Ok();
        }
    }
}
