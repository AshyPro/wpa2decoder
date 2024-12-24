using System.Text.Json;
using Ashy.Wpa2Decoder.Library;
using Ashy.Wpa2Decoder.Library.Models;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Ashy.Wpa2Decoder.Commands;

[Command("dict", Description = "Dictionary attack")]
public class DictionaryCommand : CliFx.ICommand
{
    public DictionaryCommand(IConfiguration configuration)
    {
        _config = configuration.GetSection("DictionarySettings").Get<Config>();
    }

    private readonly Config _config;
    [CommandParameter(0, IsRequired = true, Description = "parsed result json file path")]
    public string ParametersFile { get; init; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        var years = new YearsRange("2023-2025");
        if (years.YearsRangeSpecified && years.YearsRangeRange == (-1, -1))
        {
            ConsoleHelper.WriteError("Cannot parse years range.");
            return default;
        }

        var words = ValidateAndReadFileStrings(_config.WordsFile);
        var paddings = ValidateAndReadFileStrings(_config.PaddingsFile);
        var keyParametersJson = File.ReadAllText(ParametersFile);
        var keyParameters = JsonSerializer.Deserialize<PcapSummary>(keyParametersJson).KeyParametersList;
        Console.WriteLine(
            $"Words file: {_config.WordsFile}({words.Length} lines), Paddings file: {_config.PaddingsFile}({paddings.Length} lines), Pcap summary file: {ParametersFile}, Adding Years: {_config.YearsRange.YearsRangeRange.start}-{_config.YearsRange.YearsRangeRange.end}");
        
        var result = "";
        var generationTask = Task.Run(() =>
        {
            using var progressBar = new ProgressBar(new ProgressBar.RoundInfo(words.Length, ["generating", "testing"]));
            var parameters = new PasswordDictionaryGenerator.Parameters
            {
                Words = words.Distinct().ToArray(),
                Paddings = new[] { "" }.Concat(paddings).Distinct().ToArray(),
                KeyParameters = keyParameters.ToArray(),
                Years = new[] { "" }.Concat(years.GetYearsArray()).ToArray(),
                CapitalizeOnlyFirstLetter = _config.CapitalizeOnlyFirstLetter,
                MinLength = _config.MinPasswordLength,
                MaxLength = _config.MaxPasswordLength,
                Progress = progressBar,
                WordConnectors = new[] { "" }.Concat(_config.WordConnectors).Distinct().ToArray(),
                Transformations = _config.Transformations,
                Modifications = _config.Modifications
            };
            var cipher = parameters.KeyParameters.FirstOrDefault()?.Cipher;
            if (cipher != null && cipher.StartsWith("1"))
            {
                Console.WriteLine($"Deprecated cipher {cipher}");
            }

            ;
            result = PasswordDictionaryGenerator.DictionaryAttack(parameters);
        });

        // Wait for the task to finish
        generationTask.Wait();
        if (!string.IsNullOrEmpty(result))
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Beep();
            Console.WriteLine($"Password: {result}");
            Console.ResetColor();
        }
        else
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Not found.");
            Console.ResetColor();
        }

        return default;
    }

    string[] ValidateAndReadFileStrings(string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"The file '{filePath}' does not exist.");
            return [];
        }

        try
        {
            var strings = File.ReadAllLines(filePath);
            return strings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while reading the {filePath} file: {ex.Message}");
        }

        return [];
    }
}