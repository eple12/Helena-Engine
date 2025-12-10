using System.Transactions;
using H.Core;

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
    public const string DISPLAY = "d";
    public const string POSITION = "position";
    public const string POSITION_MOVES = "moves";
    public const string POSITION_STARTPOS = "startpos";
    public const string POSITION_FEN = "fen";
}

public static class UCI
{
    public static ProtocolResult ProcessCommand(string command)
    {
        string[] commandParts = command.Split(' ');
        string commandPrefix = commandParts[0];

        if (commandPrefix == ProtocolCommand.QUIT)
        {
            return ProtocolResult.QUIT;
        }

        switch (commandPrefix)
        {
            case ProtocolCommand.TEST:
                Test();
                break;

            case ProtocolCommand.DISPLAY:
                Main.MainBoard.PrintLargeBoard();
                break;

            case ProtocolCommand.POSITION:
                Position(commandParts[1..]);
                break;

            default:
                break;
        }

        return ProtocolResult.NONE;
    }

    public const string STARTPOS_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    static void Position(string[] subcommands)
    {
        if (subcommands.Length == 0)
        {
            return;
        }

        // bool containMoves = subcommands.Contains(ProtocolCommand.POSITION_MOVES);
        int moveCmdIndex = Array.FindIndex(subcommands, c => c == ProtocolCommand.POSITION_MOVES);

        string positionType = subcommands[0];
        if (positionType == ProtocolCommand.POSITION_STARTPOS)
        {
            Main.MainBoard.LoadPositionFromFEN(STARTPOS_FEN);
        }
        else if (positionType == ProtocolCommand.POSITION_FEN)
        {
            int fenEndIndex = moveCmdIndex != -1 ? moveCmdIndex : subcommands.Length;

            string fen = string.Join(' ', subcommands[1..fenEndIndex]);
            Main.MainBoard.LoadPositionFromFEN(fen);
        }
        // else.. well then the command is wrong

        if (moveCmdIndex != -1)
        {
            // Additional moves
            string[] moveList = subcommands[(moveCmdIndex + 1)..];
            foreach (string t in moveList)
            {
                System.Console.WriteLine(t);
            }
        }
    }

    static void Test()
    {
        // Board board = new Board();
        System.Console.WriteLine("asdfasdfasdfasdfasdddddddddddddddddddddddddd");
    }
}