using Ashy.Wpa2Decoder.Library.Models;

namespace Ashy.Wpa2Decoder;

public class Config
{
    /// Mame of input file with Words.
    public string WordsFile { get; set; } = "words.txt";
        
    /// Mame of file with paddings.
    public string PaddingsFile { get; set; } = "paddings.txt";
        
    /// Add years alteration. Provide a range in the format start-end.
    public YearsRange YearsRange { get; set; } = new YearsRange("2024-2025");
    
    /// Word connectors.
    public string[] WordConnectors { get; set; } = ["", "_", "-"];

    /// Character transformations.
    public Dictionary<char, string[]> Transformations { get; set; } = new() {
        {'a', ["@", "4"] },
        {'b', ["8"]},
        {'e', ["3"]},
        {'g', ["9", "6"]},
        {'i', ["1", "!"]},
        {'o', ["0", "oo"]},
        {'0', ["o", "00"]},
        {'s', ["$", "5", "z"]},
        {'t', ["7"]},
        {'r', ["rr"]},
        {'l', ["|", "1"]},
    };
    
    /// Words modifications.
    public Dictionary<string, string> Modifications { get; set; } = new() {
        {"el", "l"},
        {"ex", "x"},
        {"fo", "4"},
        {"for", "4"},
        {"ever", "evr"},
    };
    
    public bool CapitalizeOnlyFirstLetter { get; set; } = true;
    
    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 8;
}