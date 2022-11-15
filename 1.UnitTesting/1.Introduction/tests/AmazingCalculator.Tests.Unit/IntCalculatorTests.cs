namespace AmazingCalculator.Tests.Unit;

using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Xunit;

public class IntCalculatorTests
{
  private readonly IFixture _fixture;

  public IntCalculatorTests()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
  }

  [Theory, AutoMoqData]
  public void Add_ShouldAddTwoNumbers_WhenNumbersAreIntegers(IntCalculator sut)
  {
    // Act
    var result = sut.Add(1, 2);

    // Assert
    result.Should().Be(3);
  }

  [Theory, AutoMoqData]
  public void Add_ShouldReturnZero_WhenAnOppositePositiveAndNegativeNumberAreAdded(IntCalculator sut)
  {
    //Acts
    var actual = sut.Add(-5, 5);

    //Asserts
    actual.Should().Be(0);
  }
}
