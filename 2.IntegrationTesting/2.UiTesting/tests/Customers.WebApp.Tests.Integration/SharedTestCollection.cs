using Xunit;

namespace Customers.WebApp.Tests.Integration;

[CollectionDefinition("Shared collection")]
public class SharedTestCollection : ICollectionFixture<TestingContext>
{
    
}
