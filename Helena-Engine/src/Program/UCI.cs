namespace H.Program;

public enum ProtocolResult
{
    NONE,
    QUIT
}

public readonly struct ProtocolCommand
{
    public const string QUIT = "quit";
    public const string TEST = "test";
}

public static class UCI
{
    public static ProtocolResult ProcessCommand(string command)
    {
        if (command == ProtocolCommand.QUIT)
        {
            return ProtocolResult.QUIT;
        }

        switch (command)
        {
            case ProtocolCommand.TEST:
                Test();
                break;

            default:
                break;
        }

        return ProtocolResult.NONE;
    }

    static void Test()
    {
        Console.WriteLine("Testing");
    }
}