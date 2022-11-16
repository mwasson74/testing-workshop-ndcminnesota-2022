using System.Net;
using System.Net.Http.Json;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customers.Api.Tests.Integration.Controllers;

public class CustomerControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<IApiMarker> _waf = new();
    private readonly List<Guid> _idsToDelete = new();

    public CustomerControllerTests()
    {
        _client = _waf.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001")
        });
    }

    [Fact]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid()
    {
        // Arrange
        var request = new CustomerRequest
        {
            Email = "nick@chapsas.com",
            FullName = "Nick Chapsas",
            DateOfBirth = new DateTime(1993, 01, 01),
            GitHubUsername = "nickchapsas"
        };

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);
        
        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        _idsToDelete.Add(customerResponse!.Id);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        customerResponse.Should().BeEquivalentTo(request);
        response.Headers.Location.Should().Be($"https://localhost:5001/customers/{customerResponse!.Id}");
    }

    [Fact]
    public async Task Get_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var request = new CustomerRequest
        {
            Email = "nick@chapsas.com",
            FullName = "Nick Chapsas",
            DateOfBirth = new DateTime(1993, 01, 01),
            GitHubUsername = "nickchapsas"
        };
        var createResponse = await _client.PostAsJsonAsync("customers", request);
        var customerResponse = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        _idsToDelete.Add(customerResponse!.Id);
        
        // Act
        var response = await _client.GetAsync($"customers/{customerResponse.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(retrievedCustomer);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var idToDelete in _idsToDelete)
        {
            await _client.DeleteAsync($"customers/{idToDelete}");
        }
    }
}
