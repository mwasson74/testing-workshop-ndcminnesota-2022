using System.Collections.Generic;

public class AutoMoqDataAttribute : AutoDataAttribute
{
    public AutoMoqDataAttribute(bool configureMembers = true)
      : base(() => new Fixture()
        .Customize(new AutoMoqCustomization { ConfigureMembers = configureMembers }))
    { }

    public AutoMoqDataAttribute(IEnumerable<ICustomization> customizations, bool configureMembers = true)
      : base(() =>
      {
          IFixture fixture = new Fixture();
          fixture = fixture.Customize(new AutoMoqCustomization { ConfigureMembers = configureMembers });
          customizations?.ForEach(c => fixture = fixture.Customize(c));
          return fixture;
      })
    {
    }
}
