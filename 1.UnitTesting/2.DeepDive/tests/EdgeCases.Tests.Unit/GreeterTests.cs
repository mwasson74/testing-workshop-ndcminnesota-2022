using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EdgeCases.Tests.Unit;

public class GreeterTests
{
    private readonly ISystemClock _systemClock = Substitute.For<ISystemClock>();
    private readonly Greeter _sut;

    public GreeterTests()
    {
        _sut = new Greeter(_systemClock);
    }
    
    [Fact]
    public void GenerateGreetText_ShouldReturnGoodAfternoon_WhenItsAfternoon()
    {
        // Arrange
        _systemClock.Now.Returns(new DateTime(2022, 1, 1, 13, 0, 0));

        // Act
        var text = _sut.GenerateGreetText();

        // Assert
        text.Should().Be("Good afternoon");
    }
    
    [Fact]
    public void GenerateGreetText_ShouldReturnGoodMorning_WhenItsMorning()
    {
        // Arrange
        _systemClock.Now.Returns(new DateTime(2022, 1, 1, 8, 0, 0));

        // Act
        var text = _sut.GenerateGreetText();

        // Assert
        text.Should().Be("Good morning");
    }
    
    [Fact]
    public void GenerateGreetText_ShouldReturnGoodEvening_WhenItsEvening()
    {
        // Arrange
        _systemClock.Now.Returns(new DateTime(2022, 1, 1, 20, 0, 0));

        // Act
        var text = _sut.GenerateGreetText();

        // Assert
        text.Should().Be("Good evening");
    }
}
