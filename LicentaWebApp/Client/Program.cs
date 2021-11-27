using LicentaWebApp.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using LicentaWebApp.Client.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;


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
            builder.Services.AddMudServices();
            builder.Services.AddScoped<IUploadFileService, UploadFileService>();
            builder.Services.AddTransient<IUserViewModel, UserViewModel>();
            builder.Services.AddTransient<ILoginViewModel, LoginViewModel>();

            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            
            await builder.Build().RunAsync();
        }
    }
}
