using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : 
    WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    
    public HttpClient HttpClient { get; private set; } = default!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(x =>
        {
            x.ClearProviders();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task InitializeAsync()
    {
        HttpClient = CreateClient();
        _dbConnection = new NpgsqlConnection("Server=localhost;Port=5432;Database=mydb;User ID=workshop;Password=changeme;");
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

    public new Task DisposeAsync() => Task.CompletedTask;
}
