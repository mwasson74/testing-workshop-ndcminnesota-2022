using System.Data.Common;
using Customers.WebApp.Database;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Services;
using Microsoft.Playwright;
using Respawn;
using Xunit;

namespace Customers.WebApp.Tests.Integration;

public class TestingContext : IAsyncLifetime
{
    public const string AppUrl = "https://localhost:7780";
    public GitHubApiServer GitHubApiServer { get; } = new();
    
    private static readonly string DockerComposeFile = Path.Combine(Directory.GetCurrentDirectory(), (TemplateString)"../../../docker-compose.integration.yml");

    public IDbConnectionFactory Database { get; private set; }
    private IPlaywright _playwright;
    
    public IBrowserContext Browser { get; private set; }
    
    private DbConnection _respawnDbConnection = default!;
    private Respawner _respawner = default!;

    private readonly ICompositeService _dockerService = new Builder()
        .UseContainer()
        .UseCompose()
        .FromFile(DockerComposeFile)
        .RemoveOrphans()
        .WaitForHttp("test-app", AppUrl)
        .Build();
    
    public async Task InitializeAsync()
    {
        Database = new NpgsqlConnectionFactory("Server=localhost;Port=5435;Database=mydb;User ID=workshop;Password=changeme;");
        
        GitHubApiServer.Start(9850);
        _dockerService.Start();

        await InitializeRespawner();
        
        _playwright = await Playwright.CreateAsync();
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            SlowMo = 1000,
            Headless = false
        });

        Browser = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        _dockerService.Stop();
        GitHubApiServer.Dispose();
    }
    
    private async Task InitializeRespawner()
    {
        _respawnDbConnection = (DbConnection)await Database.CreateConnectionAsync();
        _respawner = await Respawner.CreateAsync(_respawnDbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_respawnDbConnection);
    }
}
