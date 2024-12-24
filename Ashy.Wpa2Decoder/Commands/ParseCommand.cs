using System.Text.Json;
using Ashy.Wpa2Decoder.Library;
using Ashy.Wpa2Decoder.Library.Models;
using CliFx.Attributes;
using CliFx.Infrastructure;

namespace Ashy.Wpa2Decoder.Commands;

[Command("parse", Description = "Parse pcap file")]
public class ParseCommand : CliFx.ICommand
{
    [CommandParameter(0, Description = "pcap file path")]
    public required string PcapFile { get; init; }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        long pcapFileSizeBytes = new FileInfo(PcapFile).Length;
        PcapSummary scanResult = new();
        var outputFileName = PcapFile.Replace(".pcap","") + ".json";
        var scanTask = Task.Run(() =>
        {
            Console.WriteLine($"Total {PcapFile} file size is {pcapFileSizeBytes/1000.0:F2} Kb");
            using var progressBar = new ProgressBar(pcapFileSizeBytes);
            var parameters = new PcapScanner.Parameters
            {
                PcapFile = PcapFile,
                TotalBytes = pcapFileSizeBytes,
                Progress = progressBar
            };
            scanResult = PcapScanner.ScanFile(parameters);
        });
        // Wait for the task to finish
        scanTask.Wait();
        var crackingParameters = PcapScanner.AnalyzeWhatCanBeCracked(scanResult);
        scanResult.KeyParametersList = crackingParameters;
        var json = JsonSerializer.Serialize(scanResult, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputFileName, json);

        PrintResultTable(scanResult);
        Console.WriteLine(crackingParameters.Any()
            ? $"All handshake data for {string.Join(",",crackingParameters.Select(x=>x.Ssid))} networks collected"
            : "No handshake data");
        Console.WriteLine($"Scan result saved to {outputFileName}");
    }
    
    static void PrintResultTable(PcapSummary scanResult)
    {
        if (scanResult.WifiNetworks.Any())
        {
            Console.WriteLine();
            // Find the maximum lengths of the keys and values to align columns
            int maxKeyLength = scanResult.WifiNetworks.Max(x => x.Ssid.Length);
            int maxValueLength = scanResult.WifiNetworks.Max(x => x.Bssid.Length);
            int maxHandshakesLength = 34;

            Console.WriteLine($"{"SSID".PadRight(maxKeyLength)} | {"BSSID".PadRight(maxValueLength)} | Handshakes");
            Console.WriteLine(new string('─', maxKeyLength + maxValueLength + maxHandshakesLength + 5));

            // Print each key-value pair in the list
            foreach (var item in scanResult.WifiNetworks)
            {
                var capturedMessages = scanResult.Handshakes.Values.FirstOrDefault(x => x.Bssid == item.Bssid)?.CapturedMessages ??
                                       string.Empty;
                if (capturedMessages.Contains("M1,M2,M3,M4"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                Console.WriteLine(
                    $"{item.Ssid.PadRight(maxKeyLength)} | {item.Bssid.PadRight(maxValueLength)} | {capturedMessages}");
                Console.ResetColor();
            }
        }
        else
        {
            Console.WriteLine("No wifi networks found");
        }
        Console.ResetColor();
        Console.WriteLine();
    }
}