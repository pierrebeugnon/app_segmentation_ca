using CANDF.Kosmos.Foundations.Client.Services.Annuaire;
using CANDF.Kosmos.Foundations.Client.Services.Habilitations;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Segmentation.Client;
using Segmentation.Client.Services;
using Segmentation.Client.Services.Application;
using Segmentation.Client.Shared;
using Segmentation.Client.Shared.Notifications;
using Segmentation.Client.Shared.Request;
using System.Globalization;
using Telerik.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

builder.Services.AddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
builder.Services.AddScoped<CookieHandler>();

builder.Services.AddHttpClient("default", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
builder.Services.AddHttpClient("Segmentation.Server", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)).AddHttpMessageHandler<CookieHandler>();
builder.Services.AddSingleton(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("default"));
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<RequestState>();
builder.Services.AddSingleton<NotificationQueue>();

builder.Services.AddSingleton<IS3Service, S3Service>();
builder.Services.AddSingleton<IHabilitationsService, HabilitationsService>();
builder.Services.AddSingleton<IUsersExtendedService, UsersExtendedService>();
builder.Services.AddSingleton<IUnitsExtendedService, UnitsExtendedService>();
builder.Services.AddSingleton<IUsersService, UsersService>();
builder.Services.AddSingleton<IUnitsService, UnitsService>();
builder.Services.AddSingleton<IHierarchiesService, HierarchiesService>();
builder.Services.AddScoped<EtpConversionService>();
builder.Services.AddScoped<RepartitionAutomatiqueService>();
builder.Services.AddScoped<SegmentationStateService>();
builder.Services.AddMudServices();


builder.Services.AddTelerikBlazor();
builder.Services.AddSingleton(typeof(ITelerikStringLocalizer), typeof(ResxLocalizer));

var host = builder.Build();

const string defaultCulture = "fr-FR";

var js = host.Services.GetRequiredService<IJSRuntime>();
var result = await js.InvokeAsync<string>("blazorCulture.get");
var culture = CultureInfo.GetCultureInfo(result ?? defaultCulture);

if (result == null)
{
	await js.InvokeVoidAsync("blazorCulture.set", defaultCulture);
}

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
