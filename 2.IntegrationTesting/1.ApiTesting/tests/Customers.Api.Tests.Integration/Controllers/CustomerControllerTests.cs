using System.Net;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Customers.Api.Tests.Integration.Controllers;

[Collection("Shared collection")]
public class CustomerControllerTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly Func<Task> _resetDb;
    private readonly GitHubApiServer _gitHubApiServer;
    private readonly Faker<CustomerRequest> _customerGenerator = 
        new Faker<CustomerRequest>()
            .RuleFor(x => x.Email, f => f.Person.Email)
            .RuleFor(x => x.FullName, f => f.Person.FullName)
            .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
            .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));

    public CustomerControllerTests(CustomerApiFactory waf)
    {
        _client = waf.HttpClient;
        _resetDb = waf.ResetDatabaseAsync;
        _gitHubApiServer = waf.GitHubApiServer;
    }

    [Fact]
    public async Task Create_ShouldCreateCustomer_WhenDetailsAreValid()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);
        
        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        customerResponse.Should().BeEquivalentTo(request);
        response.Headers.Location.Should().Be($"http://localhost/customers/{customerResponse!.Id}");
    }

    [Fact]
    public async Task Get_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);
        var createResponse = await _client.PostAsJsonAsync("customers", request);
        var customerResponse = await createResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync($"customers/{customerResponse!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(retrievedCustomer);
    }
    
    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenTheEmailIsInvalid()
    {
        // Arrange
        var request = _customerGenerator.Clone()
            .RuleFor(x => x.Email, () => "bademail")
            .Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails!.Errors["Email"].Should().Equal($"{request.Email} is not a valid email address");
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnAllCustomers_WhenCustomersExist()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);
    
        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync("customers");

        // Assert
        var customerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        customerResponse!.Customers.Should().ContainEquivalentOf(createdCustomer).And.HaveCount(1);
    }
    
    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Update_ShouldUpdateCustomerDetails_WhenDetailsAreValid()
    {
        // Arrange
        var createRequest = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(createRequest.GitHubUsername);
    
        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", createRequest);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var updateRequest = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(updateRequest.GitHubUsername);

        // Act
        var response = await _client.PutAsJsonAsync($"customers/{createdCustomer!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);    
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(updateRequest);
    }
    
    [Fact]
    public async Task Delete_ShouldDeleteCustomer_WhenCustomerExists()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);
    
        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.DeleteAsync($"customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDb();
    }
}
