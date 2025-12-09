namespace H.Program;

using System;

public static class Program
{
    public const string NAME = "Helena Engine";
    public const string VERSION = "v0.1a";

    public static int Main(string[] args)
    {
        Console.WriteLine(LOGO);
        Console.WriteLine($"{NAME} {VERSION}");

        Console.WriteLine();
        Console.WriteLine("Initializing...");
        Initialize();
        Console.WriteLine("Done.");

        while (true)
        {
            ProtocolResult result = UCI.ProcessCommand(Console.ReadLine()!);

            if (result == ProtocolResult.QUIT)
            {
                break;
            }
        }

        Console.WriteLine("Quitting...");
        
        return 0;
    }

    public static void Initialize()
    {
        
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