using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
using ChangeDetectionBlazorWebApplication.Model;

namespace ChangeDetectionBlazorWebApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(services =>
            {
                services.AddSingleton<UserRepository>();
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
