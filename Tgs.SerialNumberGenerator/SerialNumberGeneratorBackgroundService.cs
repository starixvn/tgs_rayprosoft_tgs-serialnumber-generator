using Microsoft.Extensions.Hosting;

namespace Tgs.SerialNumberGenerator;

internal class SerialNumberGeneratorBackgroundService : BackgroundService
{
	private readonly SerialNumberGenerator _numberGenerator;

	public SerialNumberGeneratorBackgroundService(SerialNumberGenerator numberGenerator)
	{
		_numberGenerator = numberGenerator;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken) => _numberGenerator.StartListenAsync();
}
