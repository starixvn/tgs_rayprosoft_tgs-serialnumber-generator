
namespace Tgs.SerialNumberGenerator;

public interface ISerialNumberGenerator
{
	ValueTask<Guid> GenerateAsync();
}