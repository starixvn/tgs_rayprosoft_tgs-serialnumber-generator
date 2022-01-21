using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.SerialNumberGenerator;

namespace SerialNumberGenerator.Benchmark;

public class GenerateTest
{
	private readonly ISerialNumberGenerator _serialNumberGenerator;

	public GenerateTest()
	{
		var builder = Host.CreateDefaultBuilder()
			.ConfigureServices(services => services.AddSerialNumberGenerator());

		var app = builder.Build();

		app.StartAsync().Wait();

		_serialNumberGenerator = app.Services.GetRequiredService<ISerialNumberGenerator>();
	}

	[Benchmark(Baseline = true)]
#pragma warning disable CA1822 // 將成員標記為靜態
	public void GenerateOrigin()
#pragma warning restore CA1822 // 將成員標記為靜態
	{
		_ = Guid.NewGuid();
	}

	[Benchmark]
	public async Task Generate1Async()
	{
		_ = await _serialNumberGenerator.GenerateAsync().ConfigureAwait(false);
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
}
