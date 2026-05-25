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

    public const string DIFFICULTY = "difficulty";

    public const string PLAY = "play";
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
            case "ucinewgame":
                engine.NewGame();
                break;

            case ProtocolCommand.BOOK:
                Book(commandParts[1..]);
                break;

            case ProtocolCommand.PRIORITY:
                Priority(commandParts[1..]);
                break;

            case ProtocolCommand.DIFFICULTY:
                Difficulty(commandParts[1..]);
                break;

            case ProtocolCommand.PLAY:
                bool wantsQuit = PlayCmd(commandParts[1..]);
                if (wantsQuit) return ProtocolResult.QUIT;
                break;

            default:
                break;
        }

        return ProtocolResult.NONE;
    }

    static void HelpMessage()
    {
        const string HR  = "  ────────────────────────────────────────────────────────────────────────────────────────────────────";
        const string HR2 = "  ════════════════════════════════════════════════════════════════════════════════════════════════════";

        void H(string title)
        {
            System.Console.WriteLine();
            System.Console.WriteLine(HR);
            System.Console.WriteLine($"  {title}");
            System.Console.WriteLine(HR);
        }
        void C(string cmd, string desc)
        {
            System.Console.WriteLine($"  {cmd,-52}  {desc}");
        }
        void E(string example)
        {
            System.Console.WriteLine($"      ex)  {example}");
        }
        void Br() => System.Console.WriteLine();

        System.Console.WriteLine();
        System.Console.WriteLine(HR2);
        System.Console.WriteLine("    Helena-Engine  —  Command Reference");
        System.Console.WriteLine(HR2);

        // ── Play ──────────────────────────────────────────────────────────────
        H("PLAY  (interactive game against the engine)");
        C("play [white|black]",                          "Start a game (default: you play White)");
        C("  [movetime <ms>]",                           "Engine think time per move (default: 3000 ms)");
        C("  [depth <n>]",                               "Engine max search depth (default: unlimited)");
        E("play black movetime 5000");
        Br();
        System.Console.WriteLine("  In-game commands:");
        C("  <move>  e2e4 / e4 / Nf3 / O-O / O-O-O",   "Enter your move (UCI or SAN)");
        C("  hint",                                       "Ask the engine for a suggestion");
        C("  undo",                                       "Take back your last move + the engine's reply");
        C("  resign",                                     "Forfeit the current game");
        C("  quit",                                       "Exit play mode (return to UCI prompt)");

        // ── Board & Position ──────────────────────────────────────────────────
        H("BOARD & POSITION");
        C("d",                                            "Display the current board");
        C("position startpos [moves m1 m2 …]",           "Load starting position, optionally with moves");
        C("position fen <FEN> [moves m1 m2 …]",          "Load a FEN position");
        C("move <m1> [m2 …]",                            "Make moves on the board directly");
        C("movegen",                                      "List all legal moves in the current position");
        C("eval",                                         "Static evaluation of the current position");

        // ── Search ────────────────────────────────────────────────────────────
        H("SEARCH");
        C("go [depth <n>]",                              "Search to a fixed depth");
        C("go movetime <ms>",                            "Search for a fixed time");
        C("go wtime <ms> btime <ms> [winc <ms> binc <ms>]", "Search with clock times");
        C("go infinite",                                  "Search indefinitely (stop with 'stop')");
        C("go perft <depth>",                            "Perft node-count test");
        C("go timedperft <depth>",                       "Timed perft test");
        C("go routineperft",                             "Run the full perft test suite");
        C("stop",                                         "Stop a running search immediately");

        // ── Settings ──────────────────────────────────────────────────────────
        H("SETTINGS");
        C("difficulty [show | list]",                    "Show current difficulty / list all levels");
        C("difficulty <name | 0-10>",                    "Set difficulty by name or number");
        C("difficulty set <name | 0-10>",                "Set difficulty (explicit form)");
        E("difficulty martin   |   difficulty 4   |   difficulty list");
        Br();
        C("book toggle",                                  "Enable / disable the opening book");
        C("book show",                                    "Show book moves for the current position");
        C("book parse",                                   "Re-parse the book data file");
        Br();
        C("priority toggle",                             "Toggle process priority (Normal ↔ High)");
        C("priority show",                               "Show current process priority");

        // ── UCI protocol ──────────────────────────────────────────────────────
        H("UCI PROTOCOL");
        C("uci",                                          "Identify engine and list UCI options");
        C("isready",                                      "Confirm the engine is ready");
        C("ucinewgame",                                   "Reset internal state for a new game");

        // ── General ───────────────────────────────────────────────────────────
        H("GENERAL");
        C("help",                                         "Show this help");
        C("quit",                                         "Exit the engine");

        // ── Debug ─────────────────────────────────────────────────────────────
        H("DEBUG");
        C("pause",                                        "Break into debugger (Debug build only)");
        C("test",                                         "Run the current internal test function");

        System.Console.WriteLine();
        System.Console.WriteLine(HR2);
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

    // ── Difficulty ────────────────────────────────────────────────────────────

    static void Difficulty(string[] subcommands)
    {
        if (subcommands.Length == 0)
        {
            ShowDifficulty();
            return;
        }

        string first = subcommands[0].ToLower();

        if (first == "show")
        {
            ShowDifficulty();
            return;
        }

        if (first == "list")
        {
            ListDifficulties();
            return;
        }

        // Accept:  "difficulty set <level>"  OR  "difficulty <level>"
        string levelToken = (first == "set" && subcommands.Length > 1)
            ? subcommands[1]
            : subcommands[0];

        if (TryParseDifficulty(levelToken, out DifficultyLevel level))
        {
            engine.SetDifficulty(level);
            ShowDifficulty();
        }
        else
        {
            System.Console.WriteLine(
                $"Unknown difficulty: '{levelToken}'. " +
                "Use 'difficulty list' to see available levels.");
        }
    }

    static void ShowDifficulty()
    {
        DifficultyLevel level = engine.GetDifficulty();
        DifficultyConfig config = DifficultySettings.Get(level);
        System.Console.WriteLine($"Difficulty: [{(int)level}] {config.Name} – {config.Description}");
    }

    static void ListDifficulties()
    {
        DifficultyLevel current = engine.GetDifficulty();
        System.Console.WriteLine("Difficulty levels:");
        System.Console.WriteLine();
        foreach (DifficultyConfig cfg in DifficultySettings.Configs)
        {
            string marker = cfg.Level == current ? "►" : " ";
            string maxNStr = cfg.MaxN == 0 ? "always best" : $"top {cfg.MaxN + 1} candidates";
            System.Console.WriteLine(
                $"  {marker} [{(int)cfg.Level,2}] {cfg.Name,-20}  {cfg.Description,-46}  ({maxNStr})");
        }
        System.Console.WriteLine();
    }

    static bool TryParseDifficulty(string token, out DifficultyLevel level)
    {
        // Numeric index  (e.g. "0", "4", "10")
        if (int.TryParse(token, out int idx)
            && idx >= 0
            && idx < DifficultySettings.Configs.Length)
        {
            level = (DifficultyLevel)idx;
            return true;
        }

        // Name or enum key (case-insensitive)
        foreach (DifficultyConfig cfg in DifficultySettings.Configs)
        {
            if (string.Equals(cfg.Name, token, StringComparison.OrdinalIgnoreCase)
                || string.Equals(cfg.Level.ToString(), token, StringComparison.OrdinalIgnoreCase))
            {
                level = cfg.Level;
                return true;
            }
        }

        level = DifficultyLevel.MAXIMUM;
        return false;
    }

    // ── Play ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses "play [white|black] [movetime &lt;ms&gt;] [depth &lt;n&gt;]" and launches PlayMode.
    /// Returns true if the user typed 'quit' inside play mode (so the main loop exits).
    /// </summary>
    static bool PlayCmd(string[] args)
    {
        bool  playerIsWhite = true;
        int   moveTimeMs    = 3000;
        int   depth         = Constants.MAX_DEPTH;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "white":
                    playerIsWhite = true;
                    break;
                case "black":
                    playerIsWhite = false;
                    break;
                case "movetime":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int mt))
                        moveTimeMs = Math.Max(100, mt);
                    break;
                case "depth":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int d))
                        depth = Math.Clamp(d, 1, Constants.MAX_DEPTH);
                    break;
            }
        }

        return PlayMode.Run(playerIsWhite, moveTimeMs, depth);
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