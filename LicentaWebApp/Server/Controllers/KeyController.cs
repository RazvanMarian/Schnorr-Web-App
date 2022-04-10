using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using LicentaWebApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace LicentaWebApp.Server.Controllers
{
    [Route("key")]
    [ApiController]
    [Authorize]
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
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var keysName = _context.Keys
                    .Where(keys => keys.UserId == currentUser.Id)
                    .Select(key => key.Name)
                    .ToList();

                if (keysName.Contains(k.Name))
                    return BadRequest("Key name already used!");
                
                var sha256 = SHA256.Create();
                var keyNameHash=BitConverter.
                    ToString(sha256.ComputeHash(Encoding.ASCII.GetBytes(k.Name)))
                    .Replace("-", "").ToLower();

                var privateFilename = "/home/razvan/keys/" + keyNameHash + "_" + currentUser.Id + ".prv";
                var publicFilename = "/home/razvan/keys/" + keyNameHash + "_" + currentUser.Id + ".pub";

                var returnCode = Generate(privateFilename, publicFilename);
                if (returnCode != 0)
                    return BadRequest("Generation error...");

                k.PrivateKeyPath = privateFilename;
                k.PublicKeyPath = publicFilename;
                
                var user = await _context.Users.Where(u => u.Id == currentUser.Id).FirstOrDefaultAsync();
                if (user != null)
                {
                    var fileContent = await System.IO.File.ReadAllTextAsync(privateFilename);
                    var salt = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
                    var iv = new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                    };
                    var k1 = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(user.Password), salt, 512);
                    var k2=new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes(user.Password), salt, 512);

                    var encKey = k1.GetBytes(32);
                    Console.WriteLine(Convert.ToBase64String(encKey));
                    var result = AesEncryptor.EncryptStringToBytes_Aes(fileContent, encKey, iv);
                    
                    var decKey=k2.GetBytes(32);
                    Console.WriteLine(Convert.ToBase64String(decKey));
                    var res = AesEncryptor.DecryptStringFromBytes_Aes(result, decKey, iv);
                    Console.WriteLine(res);
                }

                k.State = "active";
                
                if (user != null) user.Keys.Add(k);
                else
                {
                    return BadRequest("Error");
                }

                await _context.SaveChangesAsync();

                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpGet]
        [Route("getKeys")]
        public async Task<List<Key>> GetUserKeys()
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }


                return await _context.Keys.Where(k => k.UserId == currentUser.Id).ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpDelete]
        [Route("deleteKey/{keyName}")]
        public async Task<ActionResult<string>> DeleteKey(string keyName)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }


                var key = _context.Keys.FirstOrDefault(key1 => key1.UserId == currentUser.Id && key1.Name == keyName);

                if (key == null) return BadRequest("Key doesnt exist!");

                if (System.IO.File.Exists(key.PrivateKeyPath))
                    System.IO.File.Delete(key.PrivateKeyPath);

                if (System.IO.File.Exists(key.PublicKeyPath))
                    System.IO.File.Delete(key.PublicKeyPath);


                _context.Keys.Remove(key);
                await _context.SaveChangesAsync();
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        
        [HttpPost]
        [Route("renameKey")]
        public async Task<ActionResult<string>> RenameKey(Key k)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }


                var key = _context.Keys.FirstOrDefault(key1 => key1.UserId == currentUser.Id && key1.Id == k.Id);
                if (key == null) return BadRequest("Key doesn't exist!");

                if (key.Name == k.Name && key.Description == k.Description)
                    return BadRequest("There is nothing new to this key!");

                key.Name = k.Name;
                key.Description = k.Description;
                await _context.SaveChangesAsync();
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
        
    }
}