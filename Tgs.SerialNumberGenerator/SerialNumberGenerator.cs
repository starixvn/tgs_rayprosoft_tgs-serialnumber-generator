using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace Tgs.SerialNumberGenerator;

internal class SerialNumberGenerator : ISerialNumberGenerator
{
	private readonly Memory<byte> _buffer = new byte[16];
	private int _increment = 0;

	private readonly ISystemClock _systemClock;

	private readonly Channel<TaskCompletionSource<Guid>> _taskQueue = Channel.CreateUnbounded<TaskCompletionSource<Guid>>(
		new UnboundedChannelOptions
		{
			SingleReader = true
		});

	private long _previousTimestamp = 0L;

	public SerialNumberGenerator(ReadOnlySpan<byte> machineHash, ISystemClock systemClock)
	{
		var hashNumber = MemoryMarshal.AsRef<int>(machineHash);
		MemoryMarshal.Write(_buffer.Span[12..], ref hashNumber);

		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
	}

	public async ValueTask<Guid> GenerateAsync(CancellationToken cancellationToken = default)
	{
		var tcs = new TaskCompletionSource<Guid>();

		await _taskQueue.Writer.WriteAsync(tcs, cancellationToken).ConfigureAwait(false);

		_ = cancellationToken.Register(() => tcs.TrySetCanceled());

		return await tcs.Task.ConfigureAwait(false);
	}

	public Task StartListenAsync(CancellationToken cancellationToken = default) => Task.Run(async () =>
	{
		while (await _taskQueue.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
		{
			if (_taskQueue.Reader.TryRead(out var source))
				try
				{
					if (cancellationToken.IsCancellationRequested)
						_ = source.TrySetCanceled();
					else
						source.SetResult(InternalGenerateGuid());
				}
				catch (Exception ex)
				{
					_ = source.TrySetException(ex);
				}
		}
	}, cancellationToken);

	private Guid InternalGenerateGuid()
	{
		var buffer = _buffer.Span;

		Span<byte> timestampBuffer = stackalloc byte[8];
		ref var timestamp = ref MemoryMarshal.AsRef<long>(timestampBuffer);

		var currentTime = _systemClock.UtcNow;
		timestamp = currentTime.ToUnixTimeMilliseconds();
		var previousTimestamp = _previousTimestamp;
		_previousTimestamp = timestamp;

		if (timestamp != previousTimestamp)
		{
			_increment = 0;

			ref var a = ref MemoryMarshal.AsRef<int>(buffer[0..4]);
			a = MemoryMarshal.AsRef<int>(timestampBuffer[2..6]);

			ref var b = ref MemoryMarshal.AsRef<short>(buffer[4..6]);
			b = MemoryMarshal.AsRef<short>(timestampBuffer[0..2]);

			BinaryPrimitives.WriteInt16BigEndian(buffer[10..12], (short)currentTime.Year);
		}

		++_increment;

		BinaryPrimitives.WriteInt32BigEndian(buffer[6..10], _increment);

		if (_increment == int.MaxValue)
		{
			var sw = Stopwatch.StartNew();
			while (sw.ElapsedTicks < 1_0000)
				;
			sw.Stop();
		}

		var ret = new Guid(buffer);
		return ret;
	}
}
