using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
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

        [HttpPost("authenticate-credentials")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateCredentials(
            AuthenticationRequest authenticationRequest)
        {
            try
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
                        return await Task.FromResult(new AuthenticationResponse
                            { Email = authenticationRequest.EmailAddress });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return await Task.FromResult(new AuthenticationResponse() {Email = string.Empty});
        }

        [HttpPost("authenticate-generate-otp")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateGenerateOtp(
            AuthenticationRequest authenticationRequest)
        {
            try
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
                        loggedInUser.OtpCreationTime = DateTime.Now;
                        _context.SaveChanges();

                        Console.WriteLine(loggedInUser.OtpCode);
                        SendOtpMail(loggedInUser.EmailAddress, loggedInUser.OtpCode);

                        return await Task.FromResult(new AuthenticationResponse
                            { Email = authenticationRequest.EmailAddress });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }


            return await Task.FromResult(new AuthenticationResponse() {Email = string.Empty});
        }

        [HttpPost]
        [Route("authenticate-challenge-otp")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateOtp
            (AuthenticationRequest authenticationRequest)
        {
            var token = string.Empty;
            try
            {
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
                            return await Task.FromResult(new AuthenticationResponse() { Token = token });
                        }

                        token = GenerateJwtToken(loggedInUser);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return await Task.FromResult(new AuthenticationResponse() {Token = token});
        }

        [HttpPost("generate-card-code")]
        public async Task<ActionResult<AuthenticationResponse>> GenerateCardCode(
            AuthenticationRequest authenticationRequest)
        {
            try
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

                        loggedInUser.CardCodeCreationTime = DateTime.Now;
                        _context.SaveChanges();

                        var (_, helper) = CardCodeGenerator.GenerateCode(loggedInUser.Password);

                        return await Task.FromResult(new AuthenticationResponse
                            { Email = authenticationRequest.EmailAddress, helperCode = helper });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return await Task.FromResult(new AuthenticationResponse() {Email = string.Empty});
        }
        
        [HttpPost("authenticate-challenge-card")]
        public async Task<ActionResult<AuthenticationResponse>> AuthenticateCard
            (AuthenticationRequest authenticationRequest)
        {
            var token = string.Empty;
            try
            {
                var loggedInUser = await _context.Users.Where(
                    u => u.EmailAddress == authenticationRequest.EmailAddress).FirstOrDefaultAsync();


                if (loggedInUser != null)
                {
                    var (code, _) = CardCodeGenerator.GenerateCode(loggedInUser.Password);


                    authenticationRequest.SmartCardCode = authenticationRequest.SmartCardCode
                        .Take(authenticationRequest.SmartCardCode.Length - 2).ToArray();


                    if (authenticationRequest.SmartCardCode.SequenceEqual(code))
                    {
                        var now = DateTime.Now;
                        var ts = now - loggedInUser.CardCodeCreationTime;
                        if (ts.TotalSeconds > 60)
                        {
                            return await Task.FromResult(new AuthenticationResponse() { Token = token });
                        }

                        token = GenerateJwtToken(loggedInUser);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return await Task.FromResult(new AuthenticationResponse() {Token = token});
        }
        
        
        [HttpPost("get-user-by-jwt")]
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
        [Route("get-company-users")]
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
        [Route("test-auth-state")]
        public async Task<ActionResult<string>> TestAuthState()
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is { IsAuthenticated: true })
                {
                    currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Name);
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();

                if (loggedInUser == null)
                    return BadRequest("DEAD");

                var now = DateTime.Now;
                var ts = now - loggedInUser.OtpCreationTime;
                var cardTs = now - loggedInUser.CardCodeCreationTime;
                if (ts.TotalMinutes < 30)
                    return Ok("ALIVE");

                if (cardTs.TotalMinutes < 30)
                    return Ok("ALIVE");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return BadRequest("DEAD");
        }


        [HttpPost("generate-otp-code-auth")]
        [Authorize]
        public async Task<ActionResult<string>> GenerateOtpCodeAuth()
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is { IsAuthenticated: true })
                {
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();

                if (loggedInUser != null)
                {
                    loggedInUser.OtpCode = OTPGenerator.GenerateOTP();
                    loggedInUser.OtpCreationTime = DateTime.Now;
                    SendOtpMail(loggedInUser.EmailAddress, loggedInUser.OtpCode);
                    Console.WriteLine(loggedInUser.OtpCode);
                    await _context.SaveChangesAsync();

                    return Ok("Success");
                }

                return BadRequest("Error");

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }
        }


        [HttpPost("generate-card-code-auth")]
        [Authorize]
        public async Task<ActionResult<AuthenticationResponse>> GenerateCardCodeAuth()
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                
                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();

                if (loggedInUser != null)
                {
                    loggedInUser.CardCodeCreationTime=DateTime.Now;
                    _context.SaveChanges();

                    var (_, helper) = CardCodeGenerator.GenerateCode(loggedInUser.Password);
                    
                    return await Task.FromResult(new AuthenticationResponse
                        {helperCode = helper});
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }

            return await Task.FromResult(new AuthenticationResponse() {Email = string.Empty});
        }
        
        [HttpPost("authenticate-challenge-card-auth")]
        [Authorize]
        public async Task<ActionResult<string>> AuthenticateCardAuth
            (AuthenticationRequest authenticationRequest)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();
                
                if (loggedInUser != null)
                {
                    var (code, _) = CardCodeGenerator.GenerateCode(loggedInUser.Password);
                    
                    authenticationRequest.SmartCardCode = authenticationRequest.SmartCardCode
                        .Take(authenticationRequest.SmartCardCode.Length - 2).ToArray();
                    
                    if ( authenticationRequest.SmartCardCode.SequenceEqual(code))
                    {
                        var now = DateTime.Now;
                        var ts = now - loggedInUser.CardCodeCreationTime;
                        if (ts.TotalSeconds > 60)
                        {
                            return BadRequest("Error");
                        }

                        return Ok("Success!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                return null;
            }
            
            return BadRequest("Error");
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
                    if (ts.TotalSeconds > 60)
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
        
        [HttpPost]
        [Route("test-otp-auth")]
        public async Task<ActionResult<string>> TestOtpAuth([FromBody]string otpCode)
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is {IsAuthenticated: true})
                {
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
                    if (ts.TotalSeconds > 60)
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

        [HttpPost("reset-timers")]
        [Authorize]
        public async Task<ActionResult<string>> ResetCodeTimers()
        {
            try
            {
                var currentUser = new User();
                if (User.Identity is { IsAuthenticated: true })
                {
                    currentUser.Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }

                var loggedInUser = await _context.Users.Where(
                    u => u.Id == currentUser.Id).FirstOrDefaultAsync();

                if (loggedInUser != null)
                {
                    loggedInUser.OtpCreationTime = DateTime.UnixEpoch;
                    loggedInUser.CardCodeCreationTime=DateTime.UnixEpoch;
                    await _context.SaveChangesAsync();

                    return Ok("Success");
                }

                return BadRequest("Error");

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
