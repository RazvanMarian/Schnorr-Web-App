﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using LicentaWebApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
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
        
        [DllImport(ImportPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Multiple_Sign(string hash,string[] privateKeys, int signersNumber);

        private readonly UserContext _context;

        public FileController(UserContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("uploadHash")]
        public async Task<IActionResult> PostHash(byte[] hashP)
        {
            var hash = BitConverter.ToString(hashP).Replace("-", "").ToLower();

            test_sign(hash);
            Console.WriteLine("Dupa test!");
            await Task.Delay(500);
            return Ok();
        }


        [HttpPost]
        [Route("sign/file/{hash}")]
        public async Task<ActionResult<string>> SignDocument([FromRoute] string hash, [FromBody] string keyName)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var key = await _context.Keys.FirstOrDefaultAsync(k => k.UserId == currentUser.Id && k.Name == keyName);

                if (key == null) return BadRequest("No key named like this!");

                Sign_Document_Test(hash, key.PrivateKeyPath, key.PublicKeyPath);
                const string filePath = "/home/razvan/signatures/signature.plain";

                await using (var fileInput = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await fileInput.CopyToAsync(memoryStream);

                    var buffer = memoryStream.ToArray();
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
                Directory.CreateDirectory("/home/razvan/temp_files/notification"+ notification.Id);
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
        public async Task<ActionResult<string>> MultipleSignDocument([FromBody]int notificationId)
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
                foreach (var status in notification.UserStatusList)
                {
                    var key = _context.Keys
                        .FirstOrDefault(k =>
                            k.UserId == status.NotifiedUserId && k.Name == status.SelectedKeyName);
                    if (key == null)
                        return BadRequest($"Error. The key {status.SelectedKeyName} doesn't exist!");

                    if (!System.IO.File.Exists(key.PrivateKeyPath))
                        return BadRequest("Error. The key file doesn't exist!");
                    
                    keys.Add(key.PrivateKeyPath);
                }
                
                var currentUserKey = await _context.Keys.FirstOrDefaultAsync(k => k.UserId == currentUser.Id && k.Name == notification.SelectedKey);
                keys.Add(currentUserKey?.PrivateKeyPath);
         
                var path = "/home/razvan" + notification.FilePath;
                string hashString;
                
                await using (var stream = System.IO.File.OpenRead(path))
                {
                    var sha256 = SHA256.Create();
                    var hash = await sha256.ComputeHashAsync(stream);
                    hashString =  BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                
                var result = Multiple_Sign(hashString,keys.ToArray(),keys.Count);
                if (result != 0)
                    return BadRequest("Error signing the document!");

                const string signatureFilePath = "/home/razvan/signatures/signature.plain";
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
        
    }
}