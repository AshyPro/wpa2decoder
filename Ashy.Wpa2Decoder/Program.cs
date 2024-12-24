using System.Text.Json;
using Ashy.Wpa2Decoder;
using Ashy.Wpa2Decoder.Commands;
using CliFx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
// Check if the config file exists
if (!File.Exists(configFilePath))
{
    var defaultConfig = new Config(); // Create default settings
    var json = JsonSerializer.Serialize(new { DictionarySettings = defaultConfig },
        new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(configFilePath, json);
    Console.WriteLine($"Default config.json has been created at {configFilePath}.");
}

// Set up the configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)  // Set the base path
    .AddJsonFile("config.json", optional: false, reloadOnChange: true) // Load the JSON file
    .Build();

// Set up DI container
var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)  // Register configuration
    .AddTransient<ParseCommand>()// Register command
    .AddTransient<DictionaryCommand>()
    .BuildServiceProvider();

await new CliApplicationBuilder()
    .AddCommandsFromThisAssembly()
    .UseTypeActivator(commandTypes => serviceProvider)
    .Build()
    .RunAsync();
        
