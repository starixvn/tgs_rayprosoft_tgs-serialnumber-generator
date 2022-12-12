using System.Diagnostics;
using System.Threading.Channels;

namespace Tgs.SerialNumberGenerator;

internal class SerialNumberGeneratorV1 : ISerialNumberGenerator
{
	private readonly Memory<byte> _buffer;
	private readonly Memory<byte> _machineHash;

	private readonly Channel<TaskCompletionSource<Guid>> _taskQueue = Channel.CreateUnbounded<TaskCompletionSource<Guid>>(
		new UnboundedChannelOptions
		{
			SingleReader = true
		});

	private int _increment = 0;
	private long _previousTimestamp = 0L;

	public SerialNumberGeneratorV1(ReadOnlySpan<byte> machineHash)
	{
		_machineHash = machineHash[..4].ToArray();
		_buffer = new Memory<byte>(new byte[16]);

		_machineHash.Span.CopyTo(_buffer.Span[12..]);
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

		var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
		var previousTimestamp = _previousTimestamp;
		_previousTimestamp = timestamp;
		if (timestamp != previousTimestamp)
		{
			_increment = 0;

			var timestampBuffer = BitConverter.GetBytes(timestamp).AsSpan();

			timestampBuffer.Slice(4, 2).Reverse();
			timestampBuffer.Slice(6, 2).Reverse();

			timestampBuffer[..6].CopyTo(buffer);
			timestampBuffer.Slice(6, 2).CopyTo(buffer[10..]);
		}

		var serialNo = ++_increment;

		var serialBuffer = BitConverter.GetBytes(serialNo).AsSpan();

		serialBuffer.Reverse();

		serialBuffer.CopyTo(buffer[6..]);

		if (serialNo == int.MaxValue)
		{
			var sw = Stopwatch.StartNew();
			while (sw.ElapsedTicks < 10000)
				;
			sw.Stop();
		}

		return new Guid(buffer);
	}
}
