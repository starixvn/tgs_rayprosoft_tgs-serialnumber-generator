using System.Data.HashFunction.MurmurHash;
using System.Text;

namespace Tgs.SerialNumberGenerator;

public class DefaultMachineHasher : IMachineHasher
{
	private static readonly IMurmurHash3 _Hasher = MurmurHash3Factory.Instance.Create();

	public ReadOnlySpan<byte> ComputeHash()
	{
		var machineName = Environment.MachineName;

		var hashValue = _Hasher.ComputeHash(Encoding.UTF8.GetBytes(machineName));

		return hashValue.Hash;
	}
}
