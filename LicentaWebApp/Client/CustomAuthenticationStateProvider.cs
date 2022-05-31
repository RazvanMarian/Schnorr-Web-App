using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using LicentaWebApp.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace LicentaWebApp.Client
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        private readonly ILocalStorageService _localStorageService;
        public CustomAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
        }
        
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var currentUser = await GetUserByJwtAsync();

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

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async Task<User> GetUserByJwtAsync()
        {
            try
            {
                var jwtToken = await _localStorageService.GetItemAsStringAsync("jwt_token");
                if (jwtToken == null)
                    return null;

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "user/get-user-by-jwt");
                requestMessage.Content = new StringContent(jwtToken);

                requestMessage.Content.Headers.ContentType
                    = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                var returnedUser = await response.Content.ReadFromJsonAsync<User>();

                if (returnedUser != null)
                    return await Task.FromResult(returnedUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception : " + e.Message);
                await _localStorageService.ClearAsync();
                return null;
            }
            
            return null;
        }
    }
}