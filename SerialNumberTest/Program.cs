// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.SerialNumberGenerator;

var builder = Host.CreateDefaultBuilder()
	.ConfigureServices(services => services.AddSerialNumberGenerator());

var app = builder.Build();

await app.StartAsync();

var gen = app.Services.GetRequiredService<ISerialNumberGenerator>();

//Console.WriteLine(await gen.GenerateAsync());
//Console.WriteLine(await gen.GenerateAsync());
//Console.WriteLine(await gen.GenerateAsync());

var list = new List<Guid>(100_0000);
for (var i = 0; i < 100_0000; i++)
	list.Add(await gen.GenerateAsync());

foreach(var id in list.TakeLast(10000))
	Console.WriteLine(id);
