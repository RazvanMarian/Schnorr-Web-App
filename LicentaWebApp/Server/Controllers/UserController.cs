using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
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
            var claimFirstName = new Claim(ClaimTypes.Surname, user.FirstName);
            var claimLastName = new Claim(ClaimTypes.Name, user.LastName);

            var claimsIdentity = new ClaimsIdentity(new[] 
                    {claimEmail, claimNameIdentifier, claimFirstName, claimLastName},
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
            var loggedInUser = await _context.Users.Where(
                u => u.EmailAddress == authenticationRequest.EmailAddress).FirstOrDefaultAsync();

            if (loggedInUser != null)
            {
                var passwordHasher = new PasswordHasher(new HashingOptions());
                var res = passwordHasher
                    .Check(loggedInUser.Password, authenticationRequest.Password);
                if (res.Verified)
                {
                    
                    loggedInUser.OtpCode = OTPGenerator.GenerateOTP();
                    loggedInUser.OtpCreationTime=DateTime.Now;
                    _context.SaveChanges();
                    
                    Console.WriteLine(loggedInUser.OtpCode);
                    SendOtpMail(loggedInUser.EmailAddress,loggedInUser.OtpCode);
                    
                    return await Task.FromResult(new AuthenticationResponse
                        {Email = authenticationRequest.EmailAddress});
                }
            }
      

            return await Task.FromResult(new AuthenticationResponse() {Email = string.Empty});
        }

        [HttpPost]
        [Route("authenticate-challenge")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateOtp
            (AuthenticationRequest authenticationRequest)
        {
            var token = string.Empty;
            var loggedInUser = await _context.Users.Where(
                u => u.EmailAddress == authenticationRequest.EmailAddress).FirstOrDefaultAsync();
            
            if (loggedInUser != null)
            {
                if (authenticationRequest.OtpCode == loggedInUser.OtpCode)
                {
                    var now = DateTime.Now;
                    var ts = now - loggedInUser.OtpCreationTime;
                    if (ts.Seconds > 60)
                    {
                        return await Task.FromResult(new AuthenticationResponse() {Token = token});
                    }
                    
                    token = GenerateJwtToken(loggedInUser);
                }
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
        public async Task<List<User>> GetCompanyUsers()
        {
            try
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
                    .Where(u => u.Company.Id == companyId && u.Id != currentUser.Id)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost]
        [Route("generate-otp")]
        public async Task<ActionResult<string>> GenerateOtp()
        {
            var currentUser = new User();
            if (User.Identity is {IsAuthenticated: true})
            {
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            var loggedInUser = await _context.Users.Where(
                u => u.Id == currentUser.Id).FirstOrDefaultAsync();
            
            if(loggedInUser == null)
                return BadRequest("Error finding the user");
            
            var now = DateTime.Now;
            var ts = now - loggedInUser.OtpCreationTime;
            if (ts.Minutes < 30)
            {
                return Ok("ALIVE");
            }
            
            loggedInUser.OtpCode = OTPGenerator.GenerateOTP();
            loggedInUser.OtpCreationTime = DateTime.Now;
            SendOtpMail(loggedInUser.EmailAddress,loggedInUser.OtpCode);
            Console.WriteLine(loggedInUser.OtpCode);
            await _context.SaveChangesAsync();

            return Ok("Success!");
        }

        [HttpPost]
        [Route("test-otp")]
        public async Task<ActionResult<string>> TestOtp([FromBody]string otpCode)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                
                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();
                
                if(loggedInUser == null)
                    return BadRequest("Error finding the user");
                
                if (otpCode == loggedInUser.OtpCode)
                {
                    var now = DateTime.Now;
                    var ts = now - loggedInUser.OtpCreationTime;
                    if (ts.Seconds > 60)
                    {
                        return BadRequest("Otp code expired!");
                    }

                    return Ok("Success!");
                }
                return BadRequest("The code is not correct");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }
        }

        private void SendOtpMail(string email, string otp)
        {
            
            string to = email;
            const string from = "schnorrsign@gmail.com"; 
            const string password = "Minge789?";
            string mail = "Your otp code is: " + otp +".";
            MailMessage message = new MailMessage();
            message.To.Add(to);
            message.From = new MailAddress(from);
            message.Body = mail;
            message.Subject = "Schnorr Sign authentication"; 
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(from, password);
            try
            {
                smtp.Send(message);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
    }
}
