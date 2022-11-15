namespace ForeignExchange.Api.Tests.Unit;

using AutoFixture;
using AutoFixture.AutoMoq;
using Eurofins.UsFood.Testing.XunitAttributes;
using FluentAssertions;
using Services;
using Xunit;

[Trait("Category", "QuoteService")]
public class QuoteServiceTests
{
  private IFixture _fixture;

  public void TestInitialize()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
  }

  [Theory, AutoMoqData]
  public void GetQuote_DoStuff(QuoteService sut)
  {
    //Actors


    //Acts
    //var actual = await sut.GetQuoteAsync("USD", "GBP", 9);

    //Asserts
    //actual.Should().Be(9);

    sut.Invoking(async _ => await sut.GetQuoteAsync("USD", "GBP", 9)).Should().ThrowAsync<Exception>();
  }
}

