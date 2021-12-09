using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;


namespace LicentaWebApp.Server.Controllers
{
    [Route("key")]
    [ApiController]
    public class KeyController : ControllerBase
    {
        [DllImport("../../SchnorrSig/schnorrlib.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Generate(string privateFilename, string publicFilename);
        
        
        private readonly UserContext _context;
        public KeyController(UserContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("generate")]
        public async Task<ActionResult<string>> GenerateKey(Key k)
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var keysName =  await _context.Users.Join(_context.Keys, 
                u => u.Id, 
                keys => keys.UserId, 
                (u, keys) => keys.Name).ToListAsync();

            if (keysName.Contains(k.Name))
                return BadRequest("Key name already used!");
            
            var privateFilename = "/home/razvan/keys/" + k.Name + "_" + currentUser.Id + ".prv";
            var publicFilename="/home/razvan/keys/" + k.Name + "_" + currentUser.Id + ".pub";
            
            var returnCode = Generate(privateFilename, publicFilename);
            if (returnCode != 0) 
                return BadRequest("Generation error...");
            
            k.PrivateKeyPath = privateFilename;
            k.PublicKeyPath = publicFilename;
            k.State = "active";
            var user = await _context.Users.Where(u => u.Id == currentUser.Id).FirstOrDefaultAsync();
            user.Keys.Add(k);
            await _context.SaveChangesAsync();

            return Ok("Success");
        }
    }
}