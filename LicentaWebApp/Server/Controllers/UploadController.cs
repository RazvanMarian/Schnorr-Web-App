using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;


namespace LicentaWebApp.Server.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        
        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_sign(string str);
        
        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Sign_Document_Test(string hash, string privateFilename, string publicFilename);
        
        private readonly UserContext _context;
        public UploadController(UserContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        [Route("uploadHash")]
        public async Task<IActionResult> PostHash(byte[] hashP)
        {
            var hash =  BitConverter.ToString(hashP).Replace("-", "").ToLower();

            test_sign(hash);
            Console.WriteLine("Dupa test!");
            await Task.Delay(500);
            return Ok();
        }
        
        
        [HttpPost]
        [Route("sign/file/{hash}")]
        public async Task<IActionResult> SignDocument([FromRoute] string hash,[FromBody] string keyName)
        {
            Console.WriteLine("Hash = " + hash);
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var key = await _context.Keys.FirstOrDefaultAsync(k => k.UserId == currentUser.Id && k.Name == keyName);

            if (key == null) return BadRequest("No key named like this!");
            
            Sign_Document_Test(hash, key.PrivateKeyPath, key.PublicKeyPath);
            
            
            return Ok("Success");
        }
    }
}
