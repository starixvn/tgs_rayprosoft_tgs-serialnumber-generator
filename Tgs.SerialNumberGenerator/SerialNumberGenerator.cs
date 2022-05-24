using System.Collections.Concurrent;

namespace Tgs.SerialNumberGenerator;

internal class SerialNumberGenerator : ISerialNumberGenerator
{
	private readonly Memory<byte> _machineHash;
	private readonly BlockingCollection<TaskCompletionSource<Guid>> _taskQueue = new();
	private int _increment = 0;
	private long _previousTimestamp = 0L;

	public SerialNumberGenerator(ReadOnlySpan<byte> machineHash)
	{
		_machineHash = machineHash[..4].ToArray();
	}

	public async ValueTask<Guid> GenerateAsync(CancellationToken cancellationToken = default)
	{
		var tcs = new TaskCompletionSource<Guid>();

		_taskQueue.Add(tcs);

		cancellationToken.Register(() => tcs.TrySetCanceled());

		return await tcs.Task.ConfigureAwait(false);
	}

	public Task StartListenAsync(CancellationToken cancellationToken = default) => Task.Run(() =>
	{
		foreach (var source in _taskQueue.GetConsumingEnumerable(cancellationToken))
		{
			source.SetResult(InternalGenerateGuid());
		}
	}, cancellationToken);

	private Guid InternalGenerateGuid()
	{
		var buffer = (Span<byte>)stackalloc byte[16];

		var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		var previousTimestamp = Interlocked.Exchange(ref _previousTimestamp, timestamp);
		if (timestamp != previousTimestamp)
			_ = Interlocked.Exchange(ref _increment, 0);

		var timestampBuffer = BitConverter.GetBytes(timestamp).AsSpan();

		timestampBuffer.Slice(4, 2).Reverse();
		timestampBuffer.Slice(6, 2).Reverse();

		timestampBuffer[..6].CopyTo(buffer);
		timestampBuffer.Slice(6, 2).CopyTo(buffer[10..]);

		_machineHash.Span.CopyTo(buffer[12..]);

		var serialNo = Interlocked.Increment(ref _increment);

		var serialBuffer = BitConverter.GetBytes(serialNo).AsSpan();

		serialBuffer.Reverse();

		serialBuffer.CopyTo(buffer[6..]);

		if (serialNo == int.MaxValue)
			_ = SpinWait.SpinUntil(() => true, 1);

		return new Guid(buffer);
	}
}
