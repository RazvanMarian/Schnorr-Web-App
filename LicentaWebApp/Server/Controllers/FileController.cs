using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using LicentaWebApp.Shared.Utils;
using LicentaWebApp.Shared.PayloadModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Notification = DataAccessLayer.Models.Notification;
using NotificationUserStatus = DataAccessLayer.Models.NotificationUserStatus;
using User = DataAccessLayer.Models.User;


namespace LicentaWebApp.Server.Controllers
{
    [Route("file")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private const string ImportPath = "../../SchnorrSig/schnorrlib.dll";

        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Sign_Document(string hash, string privateFilename, string publicFilename);

        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Multiple_Sign(string hash, string[] privateKeys, int signersNumber);
        
        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Verify_File(string hash, string signaturePath);

        private readonly UserContext _context;

        public FileController(UserContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("sign")]
        public async Task<ActionResult<string>> SignDocument(SignPayload payload)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    currentUser.FirstName = User.FindFirstValue(ClaimTypes.Surname);
                    currentUser.LastName = User.FindFirstValue(ClaimTypes.Name);
                }

                var key = await _context.Keys.AsNoTracking().FirstOrDefaultAsync(k =>
                    k.UserId == currentUser.Id && k.Name == payload.KeyName);
                if (key == null) return BadRequest("No key named like this!");
                
                if (!System.IO.File.Exists(key.PrivateKeyPath))
                    return BadRequest("Error. The key file doesn't exist!");
                
                if (!System.IO.File.Exists(key.PublicKeyPath))
                    return BadRequest("Error. The key file doesn't exist!");

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
                
                var result = Sign_Document(payload.Hash, key.PrivateKeyPath, key.PublicKeyPath);
                
                Encryptor.EncryptFile(tempPrv,tempPass);
                Encryptor.EncryptFile(tempPub,tempPass);
                if (result != 0)
                    return BadRequest("Error signing the document");

                var notification = new Notification
                {
                    IdInitiator = currentUser.Id,
                    InitiatorFirstName = currentUser.FirstName,
                    InitiatorLastName = currentUser.LastName,
                    InitiatorEmailAddress = currentUser.EmailAddress,
                    CreatedAt = DateTime.Now,
                    Status = -1,
                    FileName = payload.FileName,
                    SelectedKey = payload.KeyName,
                    UserStatusList = null
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                
                const string publicKeyFilePath = "/home/razvan/certificates/cert.pem";
                await using (var fileInput = new FileStream(publicKeyFilePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);
                    var buffer = memoryStream.ToArray();
                    notification.PublicKey = Convert.ToBase64String(buffer);
                }

                const string filePath = "/home/razvan/signatures/signature.bin";
                await using (var fileInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);

                    var buffer = memoryStream.ToArray();
                    notification.Signature = Convert.ToBase64String(buffer);
                    
                    await _context.SaveChangesAsync();
                    return await Task.FromResult(Convert.ToBase64String(buffer));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error :{ex}");
            }
        }
        
        
        [HttpPost]
        [Route("multiple-sign-request")]
        public async Task<ActionResult<string>> MultipleSignDocumentRequest(MultipleSignPayload payload)
        {
            try
            {
                if (payload.Users == null)
                    return await Task.FromResult<ActionResult<string>>(BadRequest("Failed"));

                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    currentUser.FirstName = User.FindFirstValue(ClaimTypes.Surname);
                    currentUser.LastName = User.FindFirstValue(ClaimTypes.Name);
                }

                var notification = new Notification();
                var notificationUserStatusList = payload.Users
                    .Select(user => new NotificationUserStatus
                        {NotifiedUserId = user.Id, Status = 2, SelectedKeyName = null})
                    .ToList();

                notification.UserStatusList.AddRange(notificationUserStatusList);
                notification.IdInitiator = currentUser.Id;
                notification.InitiatorFirstName = currentUser.FirstName;
                notification.InitiatorLastName = currentUser.LastName;
                notification.InitiatorEmailAddress = currentUser.EmailAddress;
                notification.CreatedAt = DateTime.Now;
                notification.Status = payload.Users.Count;

                notification.FileName = payload.FileName;
                notification.SelectedKey = payload.UserKeyName;
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var path = "/home/razvan/temp_files/notification" + notification.Id + "/"
                           + notification.FileName;
                Directory.CreateDirectory("/home/razvan/temp_files/notification" + notification.Id);
                var fs = System.IO.File.Create(path);
                fs.Write(payload.FileContent, 0,
                    payload.FileContent.Length);
                fs.Close();

                notification.FilePath = "/temp_files/notification" + notification.Id + "/"
                                        + notification.FileName;
                foreach (var user in payload.Users)
                {
                    var u = await _context.Users.Where(x => x.Id == user.Id).FirstOrDefaultAsync();
                    u?.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }


                return await Task.FromResult<ActionResult<string>>(Ok("Success!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        [HttpPost]
        [Route("multiple-sign")]
        public async Task<ActionResult<string>> MultipleSignDocument([FromBody] int notificationId)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var notification = await _context.Notifications
                    .Include(st => st.UserStatusList)
                    .Where(n => n.Id == notificationId).FirstOrDefaultAsync();

                if (notification == null)
                {
                    return BadRequest("error");
                }

                List<string> keys = new();
                List<string> passwords = new();
                foreach (var status in notification.UserStatusList)
                {
                    var key = _context.Keys
                        .FirstOrDefault(k =>
                            k.UserId == status.NotifiedUserId && k.Name == status.SelectedKeyName);
                    if (key == null)
                        return BadRequest($"Error. The key {status.SelectedKeyName} doesn't exist!");

                    if (!System.IO.File.Exists(key.PrivateKeyPath))
                        return BadRequest("Error. The key file doesn't exist!");

                    var user =await _context.Users.FirstOrDefaultAsync(u => u.Id == status.NotifiedUserId);
                    if (user == null)
                        return BadRequest("Error. Signing user not found!");
                    
                    passwords.Add(user.Password);
                    keys.Add(key.PrivateKeyPath);
                }

                var currentUserKey = await _context.Keys.FirstOrDefaultAsync(k =>
                    k.UserId == currentUser.Id && k.Name == notification.SelectedKey);
                keys.Add(currentUserKey?.PrivateKeyPath);

                var currUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUser.Id);
                passwords.Add(currUser?.Password);

                var path = "/home/razvan" + notification.FilePath;
                string hashString;

                await using (var stream = System.IO.File.OpenRead(path))
                {
                    var sha256 = SHA256.Create();
                    var hash = await sha256.ComputeHashAsync(stream);
                    hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }

                if (keys.Count == passwords.Count)
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        var stringBuilder = new StringBuilder(keys[i]);
                        var tempPrv = stringBuilder.ToString();
                
                        stringBuilder = new StringBuilder(passwords[i]);
                        var tempPass = stringBuilder.ToString();
                        Encryptor.DecryptFile(tempPrv,tempPass);
                    }
                }
                
                var result = Multiple_Sign(hashString, keys.ToArray(), keys.Count);
                if (result != 0)
                    return BadRequest("Error signing the document!");

                if (keys.Count == passwords.Count)
                {
                    for (var i = 0; i < keys.Count; i++)
                    {
                        var stringBuilder = new StringBuilder(keys[i]);
                        var tempPrv = stringBuilder.ToString();
                
                        stringBuilder = new StringBuilder(passwords[i]);
                        var tempPass = stringBuilder.ToString();
                        Encryptor.EncryptFile(tempPrv,tempPass);
                    }
                }
                
                const string publicKeyFilePath = "/home/razvan/certificates/cert.pem";
                await using (var fileInput = new FileStream(publicKeyFilePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);
                    var buffer = memoryStream.ToArray();
                    notification.PublicKey = Convert.ToBase64String(buffer);
                }

                const string signatureFilePath = "/home/razvan/signatures/signature.bin";
                await using (var fileInput = new FileStream(signatureFilePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);
                    var buffer = memoryStream.ToArray();
                    notification.Status = -1;
                    notification.Signature = Convert.ToBase64String(buffer);
                    await _context.SaveChangesAsync();

                    return await Task.FromResult(Convert.ToBase64String(buffer));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPost]
        [Route("verify-file")]
        public async Task<ActionResult<string>> VerifyFile(VerifyFilePayload payload)
        {
            try
            {
                var path = "/home/razvan/temp_files/data.bin";
                var fs = System.IO.File.Create(path);
                fs.Write(payload.SignatureContent, 0,
                    payload.SignatureContent.Length);
                fs.Close();

                var result = Verify_File(payload.FileHash, "/home/razvan/temp_files/data.bin");
                if (result != 0)
                    return BadRequest("Error verifying the signature");

                await Task.Delay(1);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }
    }
}