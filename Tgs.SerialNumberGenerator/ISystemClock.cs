namespace Tgs.SerialNumberGenerator;

public interface ISystemClock
{
	DateTimeOffset UtcNow { get; }
}
