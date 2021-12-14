using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;
using LicentaWebApp.Shared;
using Microsoft.AspNetCore.Authentication;


namespace LicentaWebApp.Server.Controllers
{
    [Route("user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        public UserController(UserContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("loginuser")]
        public async Task<ActionResult<User>> LoginUser(User user)
        {
            user.Password = Utility.Encode(user.Password);
            Console.WriteLine(user.Password);
            User loggedInUser = await _context.Users.Where(
                u => u.EmailAddress == user.EmailAddress && u.Password == user.Password).FirstOrDefaultAsync();
            
            if (loggedInUser != null)
            {
                //create a claim
                var claimId = new Claim(ClaimTypes.NameIdentifier, loggedInUser.Id.ToString());
                //create a claim
                var claim = new Claim(ClaimTypes.Name, loggedInUser.EmailAddress);
                //create a claimsIdentity
                var claimsIdentity = new ClaimsIdentity(new[] {claimId,claim}, "serverAuth");
                //create a claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                //Sign In User
                await HttpContext.SignInAsync(claimsPrincipal);
            }
            
            return await Task.FromResult(loggedInUser);
        }

        [HttpGet]
        [Route("getcurrentuser")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            User currentUser = new User();
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
            }
            
            return await Task.FromResult(currentUser);
        }
        
        [HttpGet]
        [Route("logoutuser")]
        public async Task<ActionResult<string>> LogOutUser()
        {
            await HttpContext.SignOutAsync();
            return "Success";
        }
        
        [HttpGet]
        [Route("getusers")]
        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
