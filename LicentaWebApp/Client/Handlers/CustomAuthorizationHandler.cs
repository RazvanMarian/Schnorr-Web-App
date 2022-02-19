using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;

namespace LicentaWebApp.Client.Handlers
{

    public class CustomAuthorizationHandler : DelegatingHandler
    {
        private ILocalStorageService LocalStorageService { get; set; }

        public CustomAuthorizationHandler(ILocalStorageService localStorageService)
        {
            LocalStorageService = localStorageService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var jwtToken = await LocalStorageService.GetItemAsync<string>("jwt_token");

            if (jwtToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}