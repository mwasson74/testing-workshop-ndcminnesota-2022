namespace ForeignExchange.Api.Tests.Unit;

using AutoFixture.Xunit2;
using Eurofins.UsFood.Testing.XunitAttributes;
using FluentAssertions;
using Logging;
using Models;
using Moq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Repositories;
using Services;
using Validation;
using Xunit;

public class Nd57QuoteServiceTests
{
  private readonly QuoteService _sut;
  private readonly IRatesRepository _ratesRepository = Substitute.For<IRatesRepository>();
  private readonly ILoggerAdapter<QuoteService> _logger = Substitute.For<ILoggerAdapter<QuoteService>>();

  const string FromCurrency = "GBP";
  const string ToCurrency = "USD";

  public Nd57QuoteServiceTests()
  {
    _sut = new QuoteService(_ratesRepository, _logger);
  }

  [Fact]
  public async Task GetQuoteAsync_ShouldReturnQuote_WhenCurrenciesAreValid()
  {
    // Arrange
    var fromCurrency = "USD";
    var toCurrency = "GBP";
    var amount = 100;
    var expectedQuote = new ConversionQuote
    {
      BaseAmount = amount,
      QuoteCurrency = toCurrency,
      BaseCurrency = fromCurrency,
      QuoteAmount = 160
    };

    var expectedRate = new FxRate
    {
      FromCurrency = fromCurrency,
      ToCurrency = toCurrency,
      TimestampUtc = DateTime.UtcNow,
      Rate = 1.6m
    };

    _ratesRepository
        .GetRateAsync(fromCurrency, toCurrency)
        .Returns(expectedRate);

    // Act
    var result = await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

    // Assert
    result.Should().BeEquivalentTo(expectedQuote);
  }

  [Fact]
  public async Task GetQuoteAsync_ShouldThrowSameCurrencyException_WhenSameCurrencyIsUsed()
  {
    // Arrange
    var fromCurrency = "GBP";
    var toCurrency = "GBP";
    var amount = 100;
    var expectedRate = new FxRate
    {
      FromCurrency = fromCurrency,
      ToCurrency = toCurrency,
      TimestampUtc = DateTime.UtcNow,
      Rate = 1.6m
    };

    _ratesRepository.GetRateAsync(fromCurrency, toCurrency).Returns(expectedRate);

    // Act
    var action = () => _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

    // Assert
    await action.Should()
        .ThrowAsync<SameCurrencyException>()
        .WithMessage($"You cannot convert currency {fromCurrency} to itself");
  }

  [Fact]
  public async Task GetQuoteAsync_ShouldLogAppropriateMessage_WhenInvoked()
  {
    // Arrange
    var fromCurrency = "GBP";
    var toCurrency = "USD";
    var amount = 100;
    var expectedRate = new FxRate
    {
      FromCurrency = fromCurrency,
      ToCurrency = toCurrency,
      TimestampUtc = DateTime.UtcNow,
      Rate = 1.6m
    };

    _ratesRepository.GetRateAsync(fromCurrency, toCurrency)
        .Returns(expectedRate);

    // Act
    await _sut.GetQuoteAsync(fromCurrency, toCurrency, amount);

    // Assert
    _logger.Received(1).LogInformation("Retrieved quote for currencies {FromCurrency}->{ToCurrency} in {ElapsedMilliseconds}ms",
        Arg.Is<object[]>(x => x[0].ToString() == fromCurrency &&
                              x[1].ToString() == toCurrency));
  }


  [Theory]
  [InlineAutoMoqData(0)]
  [InlineAutoMoqData(-9)]
  public void GetQuoteAsync_ShouldThrowNegativeAmountException_WhenAmountIsZeroOrNegative(int amount, QuoteService sut)
  {
    //Acts
    //Asserts
    sut.Invoking(async _ => await sut.GetQuoteAsync(FromCurrency, ToCurrency, amount)).Should()
      .ThrowAsync<NegativeAmountException>();
  }

  [Theory, AutoMoqData]
  public async Task GetQuoteAsync_ShouldReturnNull_WhenNoRateExists([Frozen] Mock<IRatesRepository> mockRepo, QuoteService sut)
  {
    //Arrange
    _ratesRepository.GetRateAsync(FromCurrency, ToCurrency).ReturnsNull();
    mockRepo.Setup(x => x.GetRateAsync(FromCurrency, ToCurrency)).ReturnsAsync((FxRate?)null);

    //Acts
    var actual = await sut.GetQuoteAsync(FromCurrency, ToCurrency, 9);

    //Asserts
    actual.Should().BeNull();
  }
}
