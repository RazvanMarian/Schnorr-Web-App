using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.DataAccess;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;
using LicentaWebApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Utility = LicentaWebApp.Shared.Utility;


namespace LicentaWebApp.Server.Controllers
{
    [Route("user")]
    [ApiController]
    
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;

        private readonly IConfiguration _configuration;
        public UserController(UserContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["JWTSettings:SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claimEmail = new Claim(ClaimTypes.Email, user.EmailAddress);
            var claimNameIdentifier = new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());

            var claimsIdentity = new ClaimsIdentity(new[] {claimEmail, claimNameIdentifier},
                "serverAuth");


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddDays(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateJwt(
            AuthenticationRequest authenticationRequest)
        {
            var token = string.Empty;

            authenticationRequest.Password = Utility.Encode(authenticationRequest.Password);
            var loggedInUser = await _context.Users.Where(
                u => u.EmailAddress == authenticationRequest.EmailAddress && 
                     u.Password == authenticationRequest.Password).FirstOrDefaultAsync();

            if (loggedInUser != null)
            {
                token = GenerateJwtToken(loggedInUser);
            }

            return await Task.FromResult(new AuthenticationResponse() {Token = token});
        }

        [HttpPost("getuserbyjwt")]
        public async Task<ActionResult<User>> GetUserByJwt([FromBody] string jwtToken)
        {
            try
            {
                var secretKey = _configuration["JWTSettings:SecretKey"];
                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenValidatorParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                var principle = tokenHandler.ValidateToken(jwtToken, tokenValidatorParameters,
                    out var securityToken);
                var jwtSecurityToken = (JwtSecurityToken) securityToken;

                if (jwtSecurityToken != null &&
                    jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    var userId = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    return await _context.Users.Where(u => u.Id == Convert.ToInt64(userId)).FirstOrDefaultAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return null;
        }
        
        
        [HttpGet]
        [Authorize]
        [Route("getcompanyusers")]
        public async Task<List<User>> GetUsers()
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var companyId = await _context.Users
                .Where(u => u.Id == currentUser.Id)
                .Select(u => u.Company.Id).FirstOrDefaultAsync();
            
            
            return await _context.Users
                .Where(u =>u.Company.Id == companyId && u.Id != currentUser.Id)
                .ToListAsync(); 
        }
        
        [HttpGet]
        [Route("getuser/{id}")]
        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
        
    }
}
