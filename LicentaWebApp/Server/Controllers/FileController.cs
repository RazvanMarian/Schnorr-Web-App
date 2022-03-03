using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using LicentaWebApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Notification = DataAccessLayer.Models.Notification;
using NotificationUserStatus = DataAccessLayer.Models.NotificationUserStatus;
using User = DataAccessLayer.Models.User;


namespace LicentaWebApp.Server.Controllers
{
    [Route("upload")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private const string ImportPath = "../../SchnorrSig/schnorrlib.dll";
        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void test_sign(string str);
        
        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Sign_Document_Test(string hash, string privateFilename, string publicFilename);
        
        private readonly UserContext _context;
        
        private readonly IWebHostEnvironment _environment;
        public FileController(UserContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public async Task<ActionResult<string>> SignDocument([FromRoute] string hash,[FromBody] string keyName)
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var key = await _context.Keys.FirstOrDefaultAsync(k => k.UserId == currentUser.Id && k.Name == keyName);

            if (key == null) return BadRequest("No key named like this!");
            
            Sign_Document_Test(hash, key.PrivateKeyPath, key.PublicKeyPath);
            var filePath = "/home/razvan/signatures/signature.plain";

            using (var fileInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var memoryStream = new MemoryStream();
                await fileInput.CopyToAsync(memoryStream);

                var buffer = memoryStream.ToArray();
                return await Task.FromResult(Convert.ToBase64String(buffer));
            }
        }

        [HttpPost]
        [Route("multiple-sign")]
        public async Task<ActionResult<string>> MultipleSignDocument(MultipleSignPayload payload)
        {
            if (payload.Users == null)
                return await Task.FromResult<ActionResult<string>>(BadRequest("Failed"));
            Console.WriteLine("aici;");
            
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            
            
            var notification = new Notification();
            
            foreach (var user in payload.Users)
            {
               var notificationUserStatus = new NotificationUserStatus
               {
                   NotifiedUserId = user.Id,
                   Status = 2,
                   SelectedKeyName = null
               };
            
               notification.UserStatusList.Add(notificationUserStatus);
            
                await _context.SaveChangesAsync();
                
            }
            
            notification.IdInitiator = currentUser.Id;
            notification.CreatedAt = DateTime.Now;
            notification.Status = 2;
            notification.FilePath = payload.FileName;
            notification.FileName = payload.FileName;
            notification.SelectedKey = payload.UserKeyName;
            
            var path=$"{_environment.WebRootPath}/{payload.FileName}";
            var fs = System.IO.File.Create(path);
            fs.Write(payload.FileContent, 0, 
                payload.FileContent.Length);
            fs.Close();
            
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            foreach (var user in payload.Users)
            {
            
                var u = await _context.Users.Where(x => x.Id == user.Id).FirstOrDefaultAsync();
                u?.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            
 
            return await Task.FromResult<ActionResult<string>>(Ok("Success!"));
        }
    }
}
