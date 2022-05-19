using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using LicentaWebApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LicentaWebApp.Server.Controllers
{
    [Route("cert")]
    [ApiController]
    [Authorize]
    public class CertificateController : ControllerBase
    {
        private const string ImportPath = "../../SchnorrSig/schnorrlib.dll";
        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Generate_Certificate(string privateFilename, string publicFilename);
        
        private readonly UserContext _context;
        public CertificateController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("create-cert/{keyName}")]
        public async Task<ActionResult<string>> Create_Certificate(string keyName)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var key = await _context.Keys.FirstOrDefaultAsync(k => k.UserId == currentUser.Id && k.Name == keyName);
                if (key == null) return BadRequest("error");
                
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == currentUser.Id);
                if (user == null)
                    return BadRequest("Error");
                
                var stringBuilder = new StringBuilder(key.PrivateKeyPath);
                var tempPrv = stringBuilder.ToString();

                stringBuilder = new StringBuilder(key.PublicKeyPath);
                var tempPub = stringBuilder.ToString();
                
                stringBuilder = new StringBuilder(user.Password);
                var tempPass = stringBuilder.ToString();
                
                Encryptor.DecryptFile(key.PrivateKeyPath,user.Password);
                Encryptor.DecryptFile(key.PublicKeyPath,user.Password);

                
                var result = Generate_Certificate(key.PrivateKeyPath, key.PublicKeyPath);
                if (result != 0)
                    return BadRequest("error");
                var filePath = "/home/razvan/certificates/cert.pem";
                
                Encryptor.EncryptFile(tempPrv,tempPass);
                Encryptor.EncryptFile(tempPub,tempPass);

                using (var fileInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);

                    var buffer = memoryStream.ToArray();
                    return Ok(Convert.ToBase64String(buffer));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        
        
    }
}