namespace EdgeCases;

internal class Greeter
{
    private readonly ISystemClock _systemClock;

    public Greeter(ISystemClock systemClock)
    {
        _systemClock = systemClock;
    }

    public string GenerateGreetText()
    {
        var dateTimeNow = _systemClock.Now;
        return dateTimeNow.Hour switch
        {
            >= 5 and < 12 => "Good morning",
            >= 12 and < 18 => "Good afternoon",
            _ => "Good evening"
        };
    }
}
