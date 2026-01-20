using System.Diagnostics;
using System.Net.Http.Headers;
using System.Transactions;
using H.Book;
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
    public const string HELP = "help";

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

    public const string BOOK = "book";
    public const string BOOK_PARSE = "parse";
    public const string BOOK_TOGGLE = "toggle";
    public const string BOOK_SHOW = "show";

    public const string PRIORITY = "priority";
    public const string PRIORITY_SHOW = "show";
    public const string PRIORITY_TOGGLE = "toggle";
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

        if (commandPrefix == ProtocolCommand.HELP)
        {
            HelpMessage();
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
                int eval = Evaluation.Eval(verbose: true);
                System.Console.WriteLine($"Eval: {eval}");
                break;

            case ProtocolCommand.UCI:
                System.Console.WriteLine("uciok");
                break;
            case ProtocolCommand.ISREADY:
                System.Console.WriteLine("readyok");
                break;

            case ProtocolCommand.BOOK:
                Book(commandParts[1..]);
                break;

            case ProtocolCommand.PRIORITY:
                Priority(commandParts[1..]);
                break;

            default:
                break;
        }

        return ProtocolResult.NONE;
    }

    static void HelpMessage()
    {
        System.Console.WriteLine("Helena-Engine UCI+ Commands");
        System.Console.WriteLine();
        System.Console.WriteLine("UCI+ Commands:");
        System.Console.WriteLine();
        System.Console.WriteLine("help");
        System.Console.WriteLine("    - Print command guides");
        System.Console.WriteLine();
        System.Console.WriteLine("quit");
        System.Console.WriteLine("    - Quit immediately");
        System.Console.WriteLine();
        System.Console.WriteLine("uci");
        System.Console.WriteLine("    - Check the UCI protocol");
        System.Console.WriteLine();
        System.Console.WriteLine("isready");
        System.Console.WriteLine("    - Check if the engine is ready for a search");
        System.Console.WriteLine();
        System.Console.WriteLine("d");
        System.Console.WriteLine("    - Print the current board state");
        System.Console.WriteLine();
        System.Console.WriteLine("position <fen FEN | startpos> [moves move1 move2 ...]");
        System.Console.WriteLine("    - Load a position from FEN string");
        System.Console.WriteLine();
        System.Console.WriteLine("movegen");
        System.Console.WriteLine("    - Perform move generation and print out the legal moves");
        System.Console.WriteLine();
        System.Console.WriteLine("go <[depth] [infinite] [movetime] [wtime] [btime] [winc] [binc] | perft <depth> | timedperft <depth> | routineperft>");
        System.Console.WriteLine("    - Perform engine search");
        System.Console.WriteLine();
        System.Console.WriteLine("stop");
        System.Console.WriteLine("    - Stop the search immediately");
        System.Console.WriteLine();
        System.Console.WriteLine("move <move1 move2 ...>");
        System.Console.WriteLine("    - Make the moves directly");
        System.Console.WriteLine();
        System.Console.WriteLine("eval");
        System.Console.WriteLine("    - Perform static evaluation and print out the eval");
        System.Console.WriteLine();
        System.Console.WriteLine("book <toggle | show | parse>");
        System.Console.WriteLine("    - Manage opening book");
        System.Console.WriteLine();
        System.Console.WriteLine("priority <toggle | show>");
        System.Console.WriteLine("    - Manage process priority");
        System.Console.WriteLine();
        System.Console.WriteLine();

        // Debugging
        System.Console.WriteLine("Debugging Commands:");
        System.Console.WriteLine();
        System.Console.WriteLine("pause");
        System.Console.WriteLine("    - Pause the program immediately in Debug Mode");
        System.Console.WriteLine();
        System.Console.WriteLine("test");
        System.Console.WriteLine("    - Perform the current Test function");
        System.Console.WriteLine();
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

    static void Book(string[] subcommands)
    {
        string prefix = subcommands[0];
        if (prefix == ProtocolCommand.BOOK_PARSE)
        {
            BookParser.Parse();
        }
        else if (prefix == ProtocolCommand.BOOK_SHOW)
        {
            ulong key = MainBoard.State.Key;
            System.Console.WriteLine($"Key: {key}");
            BookPosition bp = H.Book.Book.TryGetBookPosition(MainBoard.State.Key);
            if (bp.IsEmpty())
            {
                System.Console.WriteLine("The book is empty.");
            }
            else
            {
                for (int i = 0; i < bp.Moves.Count; i++)
                {
                    System.Console.WriteLine($"{bp.Moves[i].Notation}: {bp.Num[i]}");
                }
            }
        }
        else if (prefix == ProtocolCommand.BOOK_TOGGLE)
        {
            engine.ToggleBook();
            System.Console.WriteLine($"Opening book {(engine.GetBookToggle() ? "enabled" : "disabled")}.");
        }
    }
    
    static void Priority(string[] subcommands)
    {
        string sub = subcommands[0];

        if (sub == ProtocolCommand.PRIORITY_TOGGLE)
        {
            Process currentProcess = Process.GetCurrentProcess();

            if (currentProcess.PriorityClass != ProcessPriorityClass.High)
            {
                currentProcess.PriorityClass = ProcessPriorityClass.High;
                System.Console.WriteLine("Priority set to High.");
            }
            else
            {
                currentProcess.PriorityClass = ProcessPriorityClass.Normal;
                System.Console.WriteLine("Priority set to Normal.");
            }
        }
        else if (sub == ProtocolCommand.PRIORITY_SHOW)
        {
            Process currentProcess = Process.GetCurrentProcess();
            System.Console.WriteLine($"Current process priority: {currentProcess.PriorityClass.ToString()}");
        }
    }

    static void Test()
    {
        // engine.Search(4);
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

        // var sw = Stopwatch.StartNew();

        // for (int i = 0; i < 1000000; i++)
        // {
        //     MainBoard.Test();
        // }

        // sw.Stop();
        // System.Console.WriteLine(sw.ElapsedMilliseconds);

        SEE see = new (MainBoard);
        // see.HasPositiveScore();
        MoveList moves = MainBoard.MoveGenerator.GenerateMoves();
        foreach (Move move in moves)
        {
            System.Console.WriteLine($"{move.Notation}: {see.HasPositiveScore(move, 0)}");
        }

        see.HasPositiveScore(new Move(SquareHelper.D1, SquareHelper.G4));

        // MoveList moves = MainBoard.MoveGenerator.GenerateMoves();
        // MoveOrdering moveOrdering = new(MainBoard, new SEE(MainBoard));
        // moveOrdering.GetOrderedMoves(ref moves, Core.Move.NullMove, false, 0);

        // System.Console.WriteLine(string.Join(' ', moves.ToArray().Select(a => a.Notation)));
    }
}