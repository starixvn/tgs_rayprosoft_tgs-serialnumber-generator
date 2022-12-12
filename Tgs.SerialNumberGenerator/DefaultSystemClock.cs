namespace Tgs.SerialNumberGenerator;

internal class DefaultSystemClock : ISystemClock
{
	public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
