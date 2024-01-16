using Caisk.App.Components;
using Caisk.Applications;
using Caisk.Data;
using Caisk.Data.LiteDb;
using Caisk.Docker;
using Caisk.GitHub;
using Caisk.Managers.Mongo;
using Caisk.SecureShells;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
{
    var services = builder.Services;
    services.AddRazorComponents()
        .AddInteractiveServerComponents();
    services.AddMudServices();
    var dbPath = GetDataPath();
    services.AddSingleton<IDataContext>(_ => new DataContext(dbPath));
    services.AddSingleton<IApplicationStore>(sp => sp.GetRequiredService<IDataContext>().ApplicationStore);
    services.AddSingleton<IRegistryStore>(sp => sp.GetRequiredService<IDataContext>().RegistryStore);
    services.AddSingleton<ISecureShellStore>(sp => sp.GetRequiredService<IDataContext>().SecureShellStore);
    services.AddSingleton<IPrivateKeyStore>(sp => sp.GetRequiredService<IDataContext>().PrivateKeyStore);
    services.AddSingleton<IMongoServerStore>(sp => sp.GetRequiredService<IDataContext>().MongoServerStore);
    services.AddSingleton<IGitHubRepositoryStore>(sp => sp.GetRequiredService<IDataContext>().GitHubRepositoryStore);
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