namespace LegacyApp.Tests.Unit;

using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using Eurofins.UsFood.Testing.XunitAttributes;
using FluentAssertions;
using Moq;
using NSubstitute;
using Xunit;

[Trait("Category", "UserService")]
public class Nd57UserServiceTests
{
  private IFixture _fixture;
  private const string FirstName = "Matt";
  private const string LastName = "Wasson";
  private const string Email = "email@email.com";
  private static readonly DateTime _dob = new(1985, 07, 17);
  private static readonly DateTime _underTwentyOne = DateTime.UtcNow.AddYears(-20);
  private const int ClientId = 9;
  private const string BadCredit = "Bad Credit";

  public Nd57UserServiceTests()
  {
    _fixture = new Fixture();
    _fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
  }

  public static IEnumerable<object[]> InvalidUserCases =>
    new List<object[]>
    {
      new object[]
      {
        string.Empty, LastName, Email, _dob, ClientId, "FirstName empty"
      },
      new object[]
      {
        FirstName, string.Empty, Email, _dob, ClientId, "LastName empty"
      },
      new object[]
      {
        FirstName, LastName, "invalidEmail", _dob, ClientId, "Invalid email"
      },
      new object[]
      {
        FirstName, LastName, Email, _underTwentyOne, ClientId, "Under 21"
      },
      new object[]
      {
        FirstName, LastName, Email, _dob, ClientId, BadCredit
      },
    };


  [Theory]
  [MemberAutoMoqData(nameof(InvalidUserCases))]
  public void AddUser_ShouldNotCreateUser(string firstName,
                                          string lastName,
                                          string email,
                                          DateTime dob,
                                          int clientId,
                                          string reason,
                                          [Frozen] Mock<IUserCreditService> mockClient,
                                          UserService sut)
  {
    if (reason.Equals(BadCredit))
    {
      mockClient.Setup(x => x.GetCreditLimit(firstName, lastName, dob).Returns(499));
    }

    //Acts
    var actual = sut.AddUser(firstName, lastName, email, dob, clientId);

    //Asserts
    actual.Should().BeFalse();
  }
}

