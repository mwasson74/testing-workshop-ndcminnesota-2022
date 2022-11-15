namespace EdgeCases;

internal class SystemClock : ISystemClock
{
    public DateTime Now => DateTime.Now;
}

internal interface ISystemClock
{
    public DateTime Now { get; }
}
