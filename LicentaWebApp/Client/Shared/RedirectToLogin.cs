using Microsoft.AspNetCore.Components;

namespace LicentaWebApp.Client.Shared;

public class RedirectToLogin : ComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo("/Login");
    }
}