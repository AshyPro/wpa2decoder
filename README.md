# Ashy.Wpa2Decoder - WiFi WPA2 Handshake Decoder

`Ashy.Wpa2Decoder` is a command-line tool designed to interact with WPA2 captured handshakes and perform operations like dictionary attacks and packet parsing. This tool is built using .NET and utilizes the `Ashy.Wpa2Decoder.Library` NuGet package for WPA2 decryption.

## Features

- **Packet Parsing**: Scan `.pcap` files to extract and analyze WPA2 EAPOL handshakes. Pcap scanning result is saved into json file with the name of original pcap. The summary will contain all the info about found wifi networks and handshake packets. It will also parse all handshake parameters like BSSID, Client MAC, ANONCE, SNONCE, and MIC.
- **Dictionary Attack**: Perform a dictionary attack on WPA2 handshakes to attempt to crack the Wi-Fi password. No external dictionary file is needed, as the tool dynamically generates and tests potential passwords based on customizable heuristics. Ashy.Wpa2Decoder.Library provides a unique feature for generating dynamic password dictionaries, removing the need for large, pre-built wordlists. This capability allows users to create and test password candidates in real-time, saving disk space and offering greater flexibility by enabling password testing with custom rules, patterns, and variations.

Check the Library [Ashy.Wpa2Decoder.Library](https://github.com/AshyPro/wpa2decoder) for more details.

## Installation

`Ashy.Wpa2Decoder` requires .NET 9.0 SDK.
The referenced NuGet package is [Add package Ashy.Wpa2Decoder.Library version 1.1.0](https://www.nuget.org/packages/Ashy.Wpa2Decoder.Library/1.1.0)

## Usage

To capture Wi-Fi traffic from your network adapter, tools like Wireshark can be used. Ensure that your adapter is set to monitor mode. Capture EAPOL packets, including Beacon and handshake messages (1 through 4). A possible Wireshark filter for this capture could be:
```
eapol || wlan.fc.type_subtype == 0x088b || wlan.fc.type_subtype == 0x0a
```

### Basic Command Syntax
```
dotnet wpa2Decoder.dll [options]
dotnet wpa2Decoder.dll [command] [arguments]
```
### Options
```
-h|--help: Shows the help text with available options and commands.
--version: Displays version information for the tool.
```
### Commands
```
dict: Perform a dictionary attack on the WPA2 handshake.
parse: Parse a .pcap file to extract WPA2 handshake information.
```
### Examples
1. Parse a .pcap File

To parse a .pcap file and extract WPA2 handshake information. Here is example:
```
wpa2Decoder parse test.pcap
Total test.pcap file size is 404,34 Kb
[########################################] 100% Completed                                                                                                                                                                                                             00:00:00 

SSID          | BSSID             | Handshakes
─────────────────────────────────────────────────────────────────────
TP-Link_Hello | 14:EB:B6:FE:FD:3D | M1,M1,M1,M1,M1,M1,M2,M3,M4
Morgen-5Ghz   | AC:61:75:8A:D5:9E | 

All handshake data for TP-Link_Hoara networks collected
Scan result saved to test.json

```
2. Perform a Dictionary Attack

To perform a dictionary attack on a captured WPA2 handshake, use the dict command with the path to a wordlist:
```
wpa2Decoder dict test.json
Words file: words.txt(2 lines), Paddings file: paddings.txt(37 lines), Pcap summary file: test.json, Adding Years: 2024-2025
Round 1 of 3: generating-[testing]
[########################################] 100% Completed                                                                                                                                                                                                             00:00:48 
Password: Hello2025

```
This command will attempt to crack the password from the WPA2 handshake using the provided wordlist.

If there is no config.json file in the current directory, the tool will generate a default one with a content like:
```
{
  "DictionarySettings": {
    "WordsFile": "words.txt",
    "PaddingsFile": "paddings.txt",
    "YearsRange": {
      "YearsRangeString": "2024-2025",
      "YearsRangeSpecified": true,
      "YearsRangeRange": {}
    },
    "WordConnectors": [
      "",
      "_",
      "-"
    ],
    "Transformations": {
      "a": ["@", "4"],
      "b": ["8"],
      "e": ["3"],
      "g": ["9", "6"],
      "i": ["1", "!"],
      "o": ["0", "oo"],
      "0": ["o", "00"],
      "s": ["$", "5", "z"],
      "t": ["7"],
      "r": ["rr"],
      "l": ["|", "1"]
    },
    "Modifications": {
      "el": "l",
      "ex": "x",
      "fo": "4",
      "for": "4",
      "ever": "evr"
    },
    "CapitalizeOnlyFirstLetter": true,
    "MinPasswordLength": 8,
    "MaxPasswordLength": 10
  }
}
```

Command Help
You can also get more specific help for each command. For example, to get help for the dict command:
```
dotnet wpa2Decoder.dll dict --help
```
## Notes
The dict command requires a valid WPA2 handshake file (.pcap) and a wordlist file to perform the dictionary attack.
The parse command will extract key parameters from the .pcap file, including BSSID, Client MAC, ANONCE, SNONCE, and MIC.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

This tool is intended for educational purposes only. It demonstrates the importance of choosing a strong password for your Wi-Fi network. Always ensure you have permission before testing or attempting to crack any network's security. The author is not responsible for any misuse of this tool.