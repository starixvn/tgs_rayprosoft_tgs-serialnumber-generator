using Microsoft.Extensions.DependencyInjection.Extensions;
using Tgs.SerialNumberGenerator;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSerialNumberGenerator(this IServiceCollection services, Action<IServiceCollection>? replaceImplementRegister = null)
	{
		services.TryAddSingleton<IMachineHasher, DefaultMachineHasher>();

		if (replaceImplementRegister is not null)
			replaceImplementRegister(services);
		else
		{
			services.TryAddSingleton<ISerialNumberGenerator>(
				sp => sp.GetRequiredService<SerialNumberGenerator>());

			_ = services
				.AddSingleton(
					sp => new SerialNumberGenerator(sp.GetRequiredService<IMachineHasher>().ComputeHash()))
				.AddHostedService<SerialNumberGeneratorBackgroundService>();
		}

		return services;
	}
}
