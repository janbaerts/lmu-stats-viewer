namespace LmuStatsViewer.Domain;

public static class Print
{
    public static void Line(string field, string value, string unit)
    {
        Console.Write(field.PadRight(35));
        Console.ForegroundColor = ConsoleColor.Blue;
        var paddedValue = (value + " " + unit).PadLeft(12);
        Console.WriteLine(paddedValue);
        Console.ResetColor();
    }

    public static void Header(string header)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n" + header);
        Console.ResetColor();
    }
}