namespace H.Program;

public static class Logger
{
    public static void LogLine(string msg, bool assert = true)
    {
        if (!assert)
        {
            return;
        }

        System.Console.WriteLine(msg);
    }
    public static void LogLine(char msg, bool assert = true)
    {
        if (!assert)
        {
            return;
        }

        System.Console.WriteLine(msg);
    }
    public static void LogLine(bool assert = true)
    {
        if (!assert)
        {
            return;
        }

        System.Console.WriteLine();
    }
    public static void Log(string msg, bool assert = true)
    {
        if (!assert)
        {
            return;
        }

        System.Console.Write(msg);
    }
    public static void Log(char msg, bool assert = true)
    {
        if (!assert)
        {
            return;
        }

        System.Console.Write(msg);
    }
    public static void Log(bool assert = true)
    {
        
    }
}