using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IO;


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

                var privateFilename = "/home/razvan/keys/" + k.Name + "_" + currentUser.Id + ".prv";
                var publicFilename = "/home/razvan/keys/" + k.Name + "_" + currentUser.Id + ".pub";

                var returnCode = Generate(privateFilename, publicFilename);
                if (returnCode != 0)
                    return BadRequest("Generation error...");

                k.PrivateKeyPath = privateFilename;
                k.PublicKeyPath = publicFilename;
                k.State = "active";
                var user = await _context.Users.Where(u => u.Id == currentUser.Id).FirstOrDefaultAsync();
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