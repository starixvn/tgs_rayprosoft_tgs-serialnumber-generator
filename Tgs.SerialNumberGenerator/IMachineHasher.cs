
namespace Tgs.SerialNumberGenerator;

public interface IMachineHasher
{
	ReadOnlySpan<byte> ComputeHash();
}