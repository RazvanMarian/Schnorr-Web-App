using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
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
                var result = Generate_Certificate(key.PrivateKeyPath, key.PublicKeyPath);
                if (result != 0)
                    return BadRequest("error");

                var filePath = "/home/razvan/certificates/cert.pem";

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