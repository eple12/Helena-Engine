namespace H.Program;

using System;

public static class Program
{
    public const string NAME = "Helena Engine";
    public const string VERSION = "v0.1a";

    public static int Main(string[] args)
    {
        Logger.LogLine(LOGO);
        Logger.LogLine($"{NAME} {VERSION}");

        Logger.LogLine();
        Logger.LogLine("Initializing...");
        Initialize();
        Logger.LogLine("Done.");
        Logger.LogLine();
        Logger.LogLine("Type \"help\" for command information.");
        Logger.LogLine();

        while (true)
        {
            ProtocolResult result = UCI.ProcessCommand(Console.ReadLine()!);

            if (result == ProtocolResult.QUIT)
            {
                break;
            }
        }

        Logger.LogLine("Quitting...");
        
        return 0;
    }

    public static void Initialize()
    {
        H.Program.Main.MainBoard.LoadPositionFromFEN(UCI.STARTPOS_FEN);
        H.Book.Book.GenerateTable();
    }

    public const string LOGO =
"""
 __  __     ______     __         ______     __   __     ______        ______     __   __     ______     __     __   __     ______    
/\ \_\ \   /\  ___\   /\ \       /\  ___\   /\ "-.\ \   /\  __ \      /\  ___\   /\ "-.\ \   /\  ___\   /\ \   /\ "-.\ \   /\  ___\   
\ \  __ \  \ \  __\   \ \ \____  \ \  __\   \ \ \-.  \  \ \  __ \     \ \  __\   \ \ \-.  \  \ \ \__ \  \ \ \  \ \ \-.  \  \ \  __\   
 \ \_\ \_\  \ \_____\  \ \_____\  \ \_____\  \ \_\\"\_\  \ \_\ \_\     \ \_____\  \ \_\\"\_\  \ \_____\  \ \_\  \ \_\\"\_\  \ \_____\ 
  \/_/\/_/   \/_____/   \/_____/   \/_____/   \/_/ \/_/   \/_/\/_/      \/_____/   \/_/ \/_/   \/_____/   \/_/   \/_/ \/_/   \/_____/ 
                                                                                                                                              
""";


}