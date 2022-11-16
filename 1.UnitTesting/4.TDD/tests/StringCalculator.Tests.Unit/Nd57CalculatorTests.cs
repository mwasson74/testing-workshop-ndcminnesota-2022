namespace StringCalculator.Tests.Unit;

using AutoFixture;
using AutoFixture.AutoMoq;
using Eurofins.UsFood.Testing.XunitAttributes;
using FluentAssertions;
using Xunit;

[Trait("Category", "StringCalculator")]
public class Nd57CalculatorTests
{
  private IFixture _fixture;

  public Nd57CalculatorTests()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
  }

  [Theory]
  [InlineAutoMoqData("", 0)]
  [InlineAutoMoqData("1,2", 3)]
  [InlineAutoMoqData("1,2,3,4,5", 15)]
  public void Add_ShouldAddAllNumbers_WhenSeparatedByComma(string numbers, int expected, Nd57Calculator sut)
  {
    //Acts
    var actual = sut.Add(numbers);

    //Asserts
    actual.Should().Be(expected);
  }
}

