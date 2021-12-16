using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using LicentaWebApp.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace LicentaWebApp.Client
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public CustomAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var currentUser = await _httpClient.GetFromJsonAsync<User>("user/getcurrentuser");

            if (currentUser != null && currentUser.EmailAddress != null)
            {
                //create a claim
                var claimId = new Claim(ClaimTypes.NameIdentifier, currentUser.Id.ToString());
                //create a claim
                var claim = new Claim(ClaimTypes.Name, currentUser.EmailAddress);
                //create a claimsIdentity
                var claimsIdentity = new ClaimsIdentity(new[] {claimId,claim}, "serverAuth");
                //create a claimsPrincipal
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }
}