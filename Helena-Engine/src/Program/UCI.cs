using System.Transactions;
using H.Core;
using H.Engine;

namespace H.Program;

public enum ProtocolResult
{
    NONE,
    QUIT
}

public readonly struct ProtocolCommand
{
    public const string PAUSE = "pause"; // For debugging
    public const string QUIT = "quit";
    public const string TEST = "test";
    public const string DISPLAY = "d";
    public const string POSITION = "position";
    public const string POSITION_MOVES = "moves";
    public const string POSITION_STARTPOS = "startpos";
    public const string POSITION_FEN = "fen";

    public const string MOVEGEN = "movegen";

    public const string GO = "go";
    public const string PERFT = "perft";
    public const string TIMED_PERFT = "timedperft";
    public const string ROUTINE_PERFT = "routineperft";

    public const string STOP = "stop";

    public const string MOVE = "move";

    public const string EVAL = "eval";

    public const string UCI = "uci";
    public const string ISREADY = "isready";
}

public static class UCI
{
    static Board MainBoard = Main.MainBoard;
    static EnginePlayer engine = Main.MainEnginePlayer;

    const int INF = Constants.INF;

    public static ProtocolResult ProcessCommand(string command)
    {
        string[] commandParts = command.Split(' ');
        string commandPrefix = commandParts[0];

        if (commandPrefix == ProtocolCommand.QUIT)
        {
            return ProtocolResult.QUIT;
        }

        if (commandPrefix == ProtocolCommand.PAUSE)
        {
            
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

            case ProtocolCommand.MOVEGEN:
                Main.MainBoard.PrintMoves();
                break;

            case ProtocolCommand.GO:
                Go(commandParts[1..]);
                break;
            case ProtocolCommand.STOP:
                engine.CancelAndWait();
                break;

            case ProtocolCommand.MOVE:
                Move(commandParts[1..]);
                break;

            case ProtocolCommand.EVAL:
                System.Console.WriteLine($"Eval: {Evaluation.Eval()}");
                break;

            case ProtocolCommand.UCI:
                System.Console.WriteLine("uciok");
                break;
            case ProtocolCommand.ISREADY:
                System.Console.WriteLine("readyok");
                break;

            default:
                break;
        }

        return ProtocolResult.NONE;
    }

    static void Go(string[] subcommands)
    {
        if (subcommands.Length == 0)
        {
            subcommands = ["infinite"];
        }

        string subPrefix = subcommands[0];

        if (subPrefix == ProtocolCommand.PERFT)
        {
            int depth = int.Parse(subcommands[1]);
            System.Console.WriteLine(Perft.GoPerft(depth, verbose: true));
        }
        else if (subPrefix == ProtocolCommand.TIMED_PERFT)
        {
            int depth = int.Parse(subcommands[1]);
            // System.Console.WriteLine(Perft.GoPerft(depth, verbose: true));
            Perft.GoTimedPerft(depth, verbose: true);
        }
        else if (subPrefix == ProtocolCommand.ROUTINE_PERFT)
        {
            // System.Console.WriteLine(Perft.GoPerft(depth, verbose: true));
            Perft.GoRoutine();
        }
        else
        {
            Search(subcommands);
        }    
    }
    static void Search(string[] tokens)
    {
        engine.CancelAndWait();
 
        int depth = Constants.MAX_DEPTH;
        int wtime = INF, btime = INF, winc = 0, binc = 0;
        int movetime = -1;
        bool infinite = false;
    
        for (int i = 0; i < tokens.Length; i++)
        {
            string subCommand = tokens[i];
            switch (subCommand)
            {
                case "depth":
                    depth = int.Parse(tokens[++i]);
                    break;
                case "infinite":
                    infinite = true;
                    break;
                case "movetime":
                    movetime = int.Parse(tokens[++i]);
                    break;
                case "wtime":
                    wtime = int.Parse(tokens[++i]);
                    break;
                case "btime":
                    btime = int.Parse(tokens[++i]);
                    break;
                case "winc":
                    winc = int.Parse(tokens[++i]);
                    break;
                case "binc":
                    binc = int.Parse(tokens[++i]);
                    break;
            }
        }

        int thinkTime;
        if (infinite)
        {
            thinkTime = -1;
        }
        else if (movetime != -1)
        {
            thinkTime = movetime;
        }
        else
        {
            thinkTime = engine.DecideThinkTime(wtime, btime, winc, binc, INF, EnginePlayer.MIN_THINKTIME);
        }
   
        Console.WriteLine($"info string searchtime {(infinite ? "infinite" : thinkTime)}");
        engine.Search(depth, thinkTime);
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
            Move(moveList);
        }
    }

    static void Move(string[] moves)
    {
        foreach (string t in moves)
        {
            Move(t);
        }
    }
    static void Move(string move)
    {
        Square start = SquareHelper.Parse(move[0..2]);
        Square target = SquareHelper.Parse(move[2..4]);

        Move[] moves = Main.MainBoard.MoveGenerator.GenerateMoves().ToArray();
        Move m;

        if (move.Length > 4)
        {
            char prom = move[4];
            PieceType type = prom switch
            {
                'n' => PieceHelper.KNIGHT,
                'b' => PieceHelper.BISHOP,
                'r' => PieceHelper.ROOK,
                'q' => PieceHelper.QUEEN,
                _ => PieceHelper.NONE
            };

            m = moves.FirstOrDefault(t => t.Start == start && t.Target == target && MoveFlag.GetPromType(t.Flag) == type, new Move(0));
        }
        else
        {
            m = moves.FirstOrDefault(t => t.Start == start && t.Target == target, new Move(0));
        }

        if (m.MoveValue == 0)
        {
            Logger.LogLine($"Invalid move: {move}");
            return;
        }

        Main.MainBoard.MakeMove(m);
    }

    static void Test()
    {
        engine.Search(4);
        // System.Console.WriteLine(TTEntry.GetSize());
        // Board board = new Board();
        // Move t1 = new(SquareHelper.E2, SquareHelper.E4, MoveFlag.PawnTwo);
        // Move t2 = new(SquareHelper.E7, SquareHelper.E5, MoveFlag.PawnTwo);
        // Move t3 = new(SquareHelper.G1, SquareHelper.F3);
        // Move t4 = new(SquareHelper.B8, SquareHelper.C6);
        // Move t5 = new(SquareHelper.F3, SquareHelper.E5, MoveFlag.Capture);

        // MainBoard.MakeMove(t1);
        // MainBoard.MakeMove(t2);
        // MainBoard.MakeMove(t3);
        // MainBoard.MakeMove(t4);
        // MainBoard.MakeMove(t5);
        // MainBoard.UnmakeMove(t5);
        // System.Console.WriteLine(Perft.GoPerft(4));
        // Perft.GoPositionAllDepth(in Perft.Perfts[5]);
    }
}