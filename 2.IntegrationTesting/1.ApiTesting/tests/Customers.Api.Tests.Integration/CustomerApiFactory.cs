using System.Data.Common;
using Bogus;
using Customers.Api.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Npgsql;
using Respawn;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : 
    WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    
    public GitHubApiServer GitHubApiServer { get; } = new();
    private readonly TestcontainerDatabase _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:latest")
            {
                Database = "mydb",
                Username = "workshop",
                Password = "changeme"
            }).Build();

    public HttpClient HttpClient { get; private set; } = default!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(x =>
        {
            x.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(_dbContainer.ConnectionString));
            
            services.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(GitHubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Workshop-{Environment.MachineName}");
            });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task InitializeAsync()
    {
        Randomizer.Seed = new Random(1);
        GitHubApiServer.Start();
        await _dbContainer.StartAsync();
        HttpClient = CreateClient();
        _dbConnection = new NpgsqlConnection(_dbContainer.ConnectionString);
        await InitializeRespawner();
    }

    private async Task InitializeRespawner()
    {
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        GitHubApiServer.Dispose();
    }
}
