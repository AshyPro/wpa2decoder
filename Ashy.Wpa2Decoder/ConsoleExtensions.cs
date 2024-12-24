namespace Ashy.Wpa2Decoder;

public static class ConsoleHelper
{
    // Extension method to write error messages in red
    public static void WriteError(string? message)
    {
        // Save the current console color
        var originalColor = Console.ForegroundColor;
        
        // Set the console color to red
        Console.ForegroundColor = ConsoleColor.Red;
        
        // Write the message
        Console.WriteLine(message);
        
        // Reset the console color to its original color
        Console.ForegroundColor = originalColor;
    }
}