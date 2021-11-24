using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;
using LicentaWebApp.Shared;

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

        [HttpGet]
        public async Task<List<User>> GetUsers()
        {
            return await _context.Users.ToListAsync<User>();
        }
    }
}
