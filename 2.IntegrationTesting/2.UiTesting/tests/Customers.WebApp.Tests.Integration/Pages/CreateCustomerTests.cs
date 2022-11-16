using Bogus;
using Customers.Api.Repositories;
using Customers.WebApp.Data;
using Customers.WebApp.Repositories;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages;

[Collection("Shared collection")]
public class CreateCustomerTests : IAsyncLifetime
{
  private readonly TestingContext _context;
  private readonly ICustomerRepository _customerRepository;
  private readonly Func<Task> _resetDb;
  private IPage _page = default!;

  private readonly Faker<CustomerDto> _customerGenerator = new Faker<CustomerDto>()
      .RuleFor(x => x.Id, Guid.NewGuid)
      .RuleFor(x => x.Email, f => f.Person.Email)
      .RuleFor(x => x.FullName, f => f.Person.FullName)
      .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
      .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));

  public CreateCustomerTests(TestingContext context)
  {
    _context = context;
    _customerRepository = new CustomerRepository(_context.Database);
    _resetDb = _context.ResetDatabaseAsync;
  }

  [Fact]
  public async Task Create_ShouldCreateCustomer_WhenDataIsValid()
  {
    // Arrange
    await _page.GotoAsync($"{TestingContext.AppUrl}/add-customer");
    var customer = _customerGenerator.Generate();

    _context.GitHubApiServer.SetupUser(customer.GitHubUsername);

    // Act
    await _page.Locator("id=fullname").FillAsync(customer.FullName);
    await _page.Locator("id=email").FillAsync(customer.Email);
    await _page.Locator("id=github-username").FillAsync(customer.GitHubUsername);
    await _page.Locator("id=dob").FillAsync(customer.DateOfBirth.ToString("yyyy-MM-dd"));
    await _page.Locator("text=Submit").ClickAsync();

    // Assert
    var href = await _page.Locator("text='here'").GetAttributeAsync("href");
    var customerIdText = href!.Replace("/customer/", string.Empty);
    var customerId = Guid.Parse(customerIdText);

    var createdCustomer = await _customerRepository.GetAsync(customerId);
    createdCustomer.Should().BeEquivalentTo(customer,
        options => options.Excluding(x => x.Id));
  }

  [Fact]
  public async Task Get_ShouldReturnCustomer_WhenCustomerExists()
  {
    // Arrange
    var customer = _customerGenerator.Generate();
    _context.GitHubApiServer.SetupUser(customer.GitHubUsername);
    await _customerRepository.CreateAsync(customer);

    // Act
    await _page.GotoAsync($"{TestingContext.AppUrl}/customer/{customer.Id}");

    // Assert
    var fullName = await _page.Locator("id=fullname-field").InnerTextAsync();
    var email = await _page.Locator("id=email-field").InnerTextAsync();
    var gitHubUserName = await _page.Locator("id=github-username-field").InnerTextAsync();
    var dateOfBirth = await _page.Locator("id=dob-field").InnerTextAsync();

    fullName.Should().Be(customer.FullName);
    email.Should().Be(customer.Email);
    gitHubUserName.Should().Be(customer.GitHubUsername);
    dateOfBirth.Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));
  }

  public async Task InitializeAsync()
  {
    _page = await _context.Browser.NewPageAsync();
  }

  public async Task DisposeAsync()
  {
    await _page.CloseAsync();
    await _resetDb();
  }
}
