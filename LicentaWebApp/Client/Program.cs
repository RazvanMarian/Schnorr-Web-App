using LicentaWebApp.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using LicentaWebApp.Client.Handlers;
using LicentaWebApp.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;


namespace LicentaWebApp.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            
            AddHttpClients(builder);
            
            builder.Services.AddMudServices();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddTransient<CustomAuthorizationHandler>();
            builder.Services.AddSingleton<IUserViewModel, UserViewModel>();
            
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            
            await builder.Build().RunAsync();
        }

        private static void AddHttpClients(WebAssemblyHostBuilder builder)
        {
            //transactional named http clients
            builder.Services.AddHttpClient<IKeyViewModel,KeyViewModel>
                    ("KeyViewModel", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<CustomAuthorizationHandler>();

            builder.Services.AddHttpClient<INotificationViewModel, NotificationViewModel>
                    ("NotificationViewModel", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<CustomAuthorizationHandler>();
            
            builder.Services.AddHttpClient<IUploadFileService, UploadFileService>
                    ("UploadFileService", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<CustomAuthorizationHandler>();

            //authentication http clients
            builder.Services.AddHttpClient<ILoginViewModel, LoginViewModel>
                ("LoginViewModelClient", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            
        }
    }
}
