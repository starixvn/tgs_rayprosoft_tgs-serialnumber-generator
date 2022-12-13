using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.SerialNumberGenerator;

namespace SerialNumberGenerator.Benchmark;

public class GenerateTest
{
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly SerialNumberGeneratorV1 _serialNumberGeneratorV1 = new(stackalloc byte[4]);

	public GenerateTest()
	{
		var builder = Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddSerialNumberGenerator());

		var app = builder.Build();

		app.StartAsync().Wait();

		_serialNumberGenerator = app.Services.GetRequiredService<ISerialNumberGenerator>();
		_ = _serialNumberGeneratorV1.StartListenAsync();
	}

	[Benchmark]
#pragma warning disable CA1822 // 將成員標記為靜態
	public void GenerateOrigin()
#pragma warning restore CA1822 // 將成員標記為靜態
	{
		_ = Guid.NewGuid();
	}

	[Benchmark(Baseline = true)]
	public async Task Generate1Async()
	{
		_ = await _serialNumberGenerator.GenerateAsync().ConfigureAwait(false);
	}

	[Benchmark]
	public async Task Generate1V1Async()
	{
		_ = await _serialNumberGeneratorV1.GenerateAsync().ConfigureAwait(false);
	}

	[Benchmark]
	public async Task Generate10WAsync()
	{
		for (var i = 0; i < 100000; i++)
		{
			_ = await _serialNumberGenerator.GenerateAsync().ConfigureAwait(false);
		}
	}

	[Benchmark]
	public async Task Generate100WAsync()
	{
		for (var i = 0; i < 1000000; i++)
		{
			_ = await _serialNumberGenerator.GenerateAsync().ConfigureAwait(false);
		}
	}

	[Benchmark]
	public async Task Generate100W_V1Async()
	{
		for (var i = 0; i < 1000000; i++)
		{
			_ = await _serialNumberGeneratorV1.GenerateAsync().ConfigureAwait(false);
		}
	}
}
