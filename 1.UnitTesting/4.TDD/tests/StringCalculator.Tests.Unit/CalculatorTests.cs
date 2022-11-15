using FluentAssertions;
using Xunit;

namespace StringCalculator.Tests.Unit;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldAddTwoNumbers_WhenTheyAreSeparatedByComma()
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = sut.Add("1,2");

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Add_ShouldReturnNumber_WhenOnlyOneNumbersIsPresent()
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = sut.Add("1");

        // Assert
        result.Should().Be(1);
    }
    
    [Fact]
    public void Add_ShouldReturnZero_WhenStringIsEmpty()
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = sut.Add(string.Empty);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData("1,2", 3)]
    [InlineData("1,2,3", 6)]
    [InlineData("1\n2,3", 6)]
    public void Add_ShouldAddAllNumbers_WhenTheyAreSeparatedByADelimiter(
        string numbers, int sum)
    {
        // Arrange
        var sut = new Calculator();

        // Act
        var result = sut.Add(numbers);

        // Assert
        result.Should().Be(sum);
    }
}
