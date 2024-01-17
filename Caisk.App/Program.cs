using Caisk.App.Components;
using Caisk.Data.LiteDb;
using Caisk.Deploy;
using Caisk.SecureShells;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
{
    var services = builder.Services;
    services.AddRazorComponents()
        .AddInteractiveServerComponents();
    services.AddMudServices();
    services.AddDataContext(GetDataPath());
    
    services.AddSingleton<DeploymentManager>();
    services.AddSingleton<SecureShellManager>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
return;

string GetDataPath()
{
    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".caisk", "caisk.db");
    Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
    Console.Write($"Data Store: {dbPath}");
    return dbPath;
}