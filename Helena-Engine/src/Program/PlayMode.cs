namespace H.Program;

using H.Core;
using H.Engine;

/// <summary>
/// Interactive console game mode: human vs Helena-Engine.
///
/// Usage (from UCI prompt):
///   play                        – you play White, engine plays Black
///   play black                  – you play Black, engine plays White
///   play white movetime 5000    – you play White, engine thinks 5 s/move
///   play black depth 8          – fixed depth 8
///
/// In-game commands:
///   [move]   e2e4 / e4 / Nf3 / O-O / O-O-O    make a move
///   hint     show engine's top suggestion
///   undo     take back the last player + engine move pair
///   resign   forfeit the game
///   quit     exit the game and return to the UCI prompt
/// </summary>
public static class PlayMode
{
    // ── Piece display ─────────────────────────────────────────────────────────
    // White pieces: uppercase (P N B R Q K)
    // Black pieces: lowercase (p n b r q k)
    // Empty square: · (a centred dot)

    // Piece colours (foreground only – no background on normal squares)
    const ConsoleColor WhitePieceFg = ConsoleColor.Yellow;
    const ConsoleColor BlackPieceFg = ConsoleColor.Cyan;

    // Last-engine-move highlight (background for the from/to squares)
    const ConsoleColor HighlightBg  = ConsoleColor.DarkGreen;
    const ConsoleColor HighlightFg  = ConsoleColor.White;

    // ── Board geometry constants ──────────────────────────────────────────────
    // Each cell is 3 chars wide: " P ", " . ", etc.  Separator char: │
    // Total rank-row width: " 8 │" (4) + 8×" P │" (4) + " 8" = 38
    const string TopBorder = "   ┌───┬───┬───┬───┬───┬───┬───┬───┐";
    const string MidBorder = "   ├───┼───┼───┼───┼───┼───┼───┼───┤";
    const string BotBorder = "   └───┴───┴───┴───┴───┴───┴───┴───┘";

    // ── Entry point ────────────────────────────────────────────────────────────

    /// <returns>true if the user typed 'quit' (caller should exit the program).</returns>
    public static bool Run(bool playerIsWhite, int moveTimeMs, int maxDepth)
    {
        Board        board  = Main.MainBoard;
        EnginePlayer engine = Main.MainEnginePlayer;

        board.LoadPositionFromFEN(UCI.STARTPOS_FEN);
        engine.NewGame();

        DifficultyConfig diffCfg = DifficultySettings.Get(engine.GetDifficulty());

        var  history        = new List<Move>();
        Move lastEngineMove = Move.NullMove;
        bool wantQuit       = false;

        PrintWelcome(playerIsWhite, diffCfg, moveTimeMs, maxDepth);

        // ── Main game loop ────────────────────────────────────────────────────
        while (true)
        {
            PrintBoard(board, playerIsWhite, lastEngineMove);

            // Game-end checks
            MoveList legal = board.MoveGenerator.GenerateMoves();
            if (legal.Length == 0)
            {
                if (board.InCheck())
                    PrintResult(board.State.SideToMove ? "Black wins by checkmate!  0-1" : "White wins by checkmate!  1-0");
                else
                    PrintResult("Stalemate - Draw  1/2-1/2");
                break;
            }
            if (board.IsFiftyMoveDraw())         { PrintResult("Draw by 50-move rule  1/2-1/2");            break; }
            if (board.IsRepetition())             { PrintResult("Draw by repetition  1/2-1/2");              break; }
            if (board.IsInsufficientMaterial())   { PrintResult("Draw by insufficient material  1/2-1/2");   break; }

            bool isPlayerTurn = board.State.SideToMove == playerIsWhite;

            if (isPlayerTurn)
            {
                // ── Human turn ────────────────────────────────────────────────
                PrintTurnHeader(board, playerIsWhite);
                PrintLegalMoveList(legal);

                while (true)
                {
                    Console.Write("\n  Your move > ");
                    string raw = Console.ReadLine()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(raw)) continue;

                    string lo = raw.ToLower();

                    if (lo is "quit" or "exit" or "q")  { wantQuit = true; break; }

                    if (lo is "resign" or "r")
                    {
                        PrintResult(playerIsWhite ? "You resigned.  0-1" : "You resigned.  1-0");
                        goto GameOver;
                    }

                    if (lo is "hint" or "h")
                    {
                        ShowHint(engine, moveTimeMs, maxDepth);
                        continue;
                    }

                    if (lo == "undo")
                    {
                        if (history.Count < 2)
                        {
                            Cw("  Nothing to undo.\n", ConsoleColor.DarkYellow);
                            continue;
                        }
                        board.UnmakeMove(history[^1]); history.RemoveAt(history.Count - 1);
                        board.UnmakeMove(history[^1]); history.RemoveAt(history.Count - 1);
                        lastEngineMove = history.Count >= 1 ? history[^1] : Move.NullMove;
                        Cw("  Undo successful.\n", ConsoleColor.Green);
                        break;
                    }

                    Move? parsed = TryParseMoveInput(raw, board);
                    if (parsed is null || parsed.Value.MoveValue == 0)
                    {
                        Cw($"  Invalid move: '{raw}'. Try e2e4, e4, Nf3, O-O …\n", ConsoleColor.Red);
                        continue;
                    }

                    board.MakeMove(parsed.Value);
                    history.Add(parsed.Value);
                    break;
                }

                if (wantQuit) break;
            }
            else
            {
                // ── Engine turn ───────────────────────────────────────────────
                string side = playerIsWhite ? "Black (Engine)" : "White (Engine)";
                Console.Write($"\n  {side} is thinking…");
                Console.Out.Flush();

                Move engineMove = Move.NullMove;
                using var done  = new System.Threading.ManualResetEventSlim(false);

                engine.SetBestMoveCallback(m => { engineMove = m; done.Set(); });
                engine.Search(maxDepth, moveTimeMs);
                done.Wait();
                engine.SetBestMoveCallback(null);

                Console.WriteLine();

                if (engineMove.MoveValue == 0)
                {
                    Cw("  Engine returned no move.\n", ConsoleColor.Red);
                    break;
                }

                board.MakeMove(engineMove);
                history.Add(engineMove);
                lastEngineMove = engineMove;

                string desc = FormatMoveDescription(engineMove, board);
                Console.Write("  Engine played: ");
                Cw(desc, ConsoleColor.Cyan);
                Console.WriteLine();
            }
        }

        GameOver:
        Console.WriteLine();
        Cw("  Returned to UCI mode. Type 'play' to start a new game.\n\n", ConsoleColor.DarkGray);
        return wantQuit;
    }

    // ── Board rendering ────────────────────────────────────────────────────────

    static void PrintBoard(Board board, bool whitePov, Move lastEngineMove)
    {
        Console.WriteLine();
        PrintFileLabels(whitePov);
        Console.WriteLine(TopBorder);

        for (int ri = 0; ri < 8; ri++)
        {
            int rank = whitePov ? 7 - ri : ri;

            Console.Write($" {rank + 1} │");

            for (int fi = 0; fi < 8; fi++)
            {
                int    file = whitePov ? fi : 7 - fi;
                Square sq   = SquareHelper.GetSquare(file, rank);
                Piece  p    = board.At(sq);

                bool isHigh = lastEngineMove.MoveValue != 0
                    && (sq == lastEngineMove.Start || sq == lastEngineMove.Target);

                if (isHigh)
                    Console.BackgroundColor = HighlightBg;

                if (p == PieceHelper.NONE)
                {
                    Console.Write("   ");
                }
                else
                {
                    Console.ForegroundColor = isHigh ? HighlightFg
                                            : PieceHelper.IsColor(p, PieceHelper.WHITE) ? WhitePieceFg
                                            : BlackPieceFg;
                    Console.Write($" {PieceHelper.ToChar(p)} ");
                }

                Console.ResetColor();
                Console.Write("│");
            }

            Console.WriteLine($" {rank + 1}");

            if (ri < 7) Console.WriteLine(MidBorder);
        }

        Console.WriteLine(BotBorder);
        PrintFileLabels(whitePov);
        Console.WriteLine();
    }

    static void PrintFileLabels(bool whitePov)
    {
        // "     " = 5 spaces aligns the letter with the centre of each 3-wide cell.
        Console.Write("     ");
        for (int fi = 0; fi < 8; fi++)
        {
            int file = whitePov ? fi : 7 - fi;
            Console.Write((char)('a' + file));
            if (fi < 7) Console.Write("   ");
        }
        Console.WriteLine();
    }

    // ── Status helpers ─────────────────────────────────────────────────────────

    static void PrintWelcome(bool playerIsWhite, DifficultyConfig diff, int moveTimeMs, int depth)
    {
        Console.WriteLine();
        Cw("  ┌──────────────────────────────────────┐\n", ConsoleColor.DarkCyan);
        Cw("  |       Helena-Engine  Play Mode       |\n", ConsoleColor.DarkCyan);
        Cw("  └──────────────────────────────────────┘\n", ConsoleColor.DarkCyan);
        Console.WriteLine();
        Console.WriteLine($"  You play  : {(playerIsWhite ? "White (P N B R Q K)" : "Black (p n b r q k)")}");
        Console.WriteLine($"  Difficulty: [{(int)diff.Level}] {diff.Name}  -  {diff.Description}");
        Console.WriteLine($"  Think time: {moveTimeMs / 1000.0:F1} s/move" +
                          $"  |  Max depth: {(depth >= Constants.MAX_DEPTH ? "unlimited" : depth.ToString())}");
        Console.WriteLine();
        Cw("  Commands: resign  undo  hint  quit\n\n", ConsoleColor.DarkGray);
    }

    static void PrintTurnHeader(Board board, bool playerIsWhite)
    {
        string side = playerIsWhite ? "White (P N B R Q K)" : "Black (p n b r q k)";
        Console.Write($"\n  Your turn - {side}");
        if (board.InCheck()) { Console.Write("  "); Cw("CHECK!", ConsoleColor.Red); }
        Console.WriteLine();
    }

    static void PrintLegalMoveList(MoveList legal)
    {
        var arr      = legal.ToArray();
        var captures = arr.Where(m => MoveFlag.IsCapture(m.Flag)).Select(m => m.Notation);
        var quiet    = arr.Where(m => !MoveFlag.IsCapture(m.Flag)).Select(m => m.Notation);
        string all   = string.Join(" ", captures.Concat(quiet));

        Console.Write($"  Legal moves ({arr.Length}): ");
        const int wrap = 70;
        int col = 16;
        foreach (string tok in all.Split(' '))
        {
            if (col + tok.Length + 1 > wrap)
            {
                Console.Write("\n    " + new string(' ', 14));
                col = 18;
            }
            Console.Write(tok + " ");
            col += tok.Length + 1;
        }
        Console.WriteLine();
    }

    static void PrintResult(string message)
    {
        Console.WriteLine();
        Cw($"  == {message} ==\n\n", ConsoleColor.Yellow);
    }

    // ── Hint ──────────────────────────────────────────────────────────────────

    static void ShowHint(EnginePlayer engine, int moveTimeMs, int depth)
    {
        Console.Write("\n  Calculating hint…");
        Console.Out.Flush();

        Move best = Move.NullMove;
        using var done = new System.Threading.ManualResetEventSlim(false);

        engine.SetBestMoveCallback(m => { best = m; done.Set(); });
        engine.Search(depth, Math.Min(moveTimeMs, 2000));
        done.Wait();
        engine.SetBestMoveCallback(null);

        Console.WriteLine();
        if (best.MoveValue != 0)
        {
            Console.Write("  Hint: ");
            Cw(best.Notation + "\n", ConsoleColor.Green);
        }
    }

    // ── Move description ──────────────────────────────────────────────────────

    static string FormatMoveDescription(Move m, Board board)
    {
        // Board is already AFTER the move; unmake temporarily to read the piece
        board.UnmakeMove(m);
        string name = PieceHelper.GetPieceType(board.At(m.Start)) switch
        {
            PieceHelper.PAWN   => "Pawn",
            PieceHelper.KNIGHT => "Knight",
            PieceHelper.BISHOP => "Bishop",
            PieceHelper.ROOK   => "Rook",
            PieceHelper.QUEEN  => "Queen",
            PieceHelper.KING   => "King",
            _                  => "?"
        };
        string desc = MoveFlag.IsCastling(m.Flag)
            ? (m.Flag == MoveFlag.KCastling ? "O-O (kingside castle)" : "O-O-O (queenside castle)")
            : MoveFlag.IsPromotion(m.Flag) ? $"Pawn promotes  {m.Notation}"
            : MoveFlag.IsCapture(m.Flag)   ? $"{name} {m.Notation[..2]} x {m.Notation[2..4]}"
            : $"{name} {m.Notation}";
        board.MakeMove(m);   // restore
        return desc;
    }

    // ── Move input parsing ────────────────────────────────────────────────────

    /// <summary>
    /// Accepts UCI (e2e4, e7e8q), short pawn (e4, exd5, e8q),
    /// piece SAN (Nf3, Bxc4, Nbd7) and castling (O-O, 0-0, O-O-O, 0-0-0).
    /// Returns null if the input is not recognisable.
    /// </summary>
    static Move? TryParseMoveInput(string raw, Board board)
    {
        MoveList legal   = board.MoveGenerator.GenerateMoves();
        Move[]   legalArr = legal.ToArray();
        string   lo       = raw.ToLower();

        // ── Castling ──────────────────────────────────────────────────────────
        if (lo is "o-o" or "0-0")
        {
            var m = legalArr.FirstOrDefault(mv => mv.Flag == MoveFlag.KCastling, Move.NullMove);
            return m.MoveValue != 0 ? m : null;
        }
        if (lo is "o-o-o" or "0-0-0")
        {
            var m = legalArr.FirstOrDefault(mv => mv.Flag == MoveFlag.QCastling, Move.NullMove);
            return m.MoveValue != 0 ? m : null;
        }

        // ── UCI  e2e4 / e7e8q ─────────────────────────────────────────────────
        if (lo.Length >= 4
            && char.IsLetter(lo[0]) && char.IsDigit(lo[1])
            && char.IsLetter(lo[2]) && char.IsDigit(lo[3]))
        {
            Square start  = SquareHelper.Parse(lo[0..2]);
            Square target = SquareHelper.Parse(lo[2..4]);
            if (start != SquareHelper.INVALID_SQUARE && target != SquareHelper.INVALID_SQUARE)
            {
                if (lo.Length == 5)
                {
                    PieceType pt = lo[4] switch {
                        'q' => PieceHelper.QUEEN,  'r' => PieceHelper.ROOK,
                        'b' => PieceHelper.BISHOP, 'n' => PieceHelper.KNIGHT,
                        _   => PieceHelper.NONE };
                    if (pt != PieceHelper.NONE)
                    {
                        var pm = legalArr.FirstOrDefault(
                            mv => mv.Start == start && mv.Target == target && MoveFlag.GetPromType(mv.Flag) == pt,
                            Move.NullMove);
                        return pm.MoveValue != 0 ? pm : null;
                    }
                }
                var m = legalArr.FirstOrDefault(
                    mv => mv.Start == start && mv.Target == target, Move.NullMove);
                if (m.MoveValue == 0) return null;
                // If ambiguous (promotion), default to queen
                if (MoveFlag.IsPromotion(m.Flag) && MoveFlag.GetPromType(m.Flag) != PieceHelper.QUEEN)
                {
                    var qm = legalArr.FirstOrDefault(
                        mv => mv.Start == start && mv.Target == target
                           && MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN, m);
                    return qm;
                }
                return m;
            }
        }

        // ── Pawn capture  exd5 / bxc3 ─────────────────────────────────────────
        if (lo.Length >= 4 && lo[1] == 'x')
        {
            int    srcFile = lo[0] - 'a';
            Square target  = SquareHelper.Parse(lo[2..4]);
            if (srcFile >= 0 && srcFile <= 7 && target != SquareHelper.INVALID_SQUARE)
            {
                var cands = legalArr.Where(mv =>
                    mv.Target == target
                    && SquareHelper.GetFile(mv.Start) == srcFile
                    && PieceHelper.GetPieceType(board.At(mv.Start)) == PieceHelper.PAWN
                    && MoveFlag.IsCapture(mv.Flag)).ToArray();
                if (cands.Length > 0)
                {
                    if (lo.Length == 5)
                    {
                        PieceType pt = lo[4] switch {
                            'q' => PieceHelper.QUEEN,  'r' => PieceHelper.ROOK,
                            'b' => PieceHelper.BISHOP, 'n' => PieceHelper.KNIGHT,
                            _   => PieceHelper.QUEEN };
                        return cands.FirstOrDefault(mv => MoveFlag.GetPromType(mv.Flag) == pt, cands[0]);
                    }
                    return cands.FirstOrDefault(
                        mv => !MoveFlag.IsPromotion(mv.Flag) || MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN,
                        cands[0]);
                }
            }
        }

        // ── Short pawn move  e4 / e8 / e8q ───────────────────────────────────
        if (lo.Length >= 2 && char.IsLetter(lo[0]) && char.IsDigit(lo[1]))
        {
            Square target = SquareHelper.Parse(lo[0..2]);
            if (target != SquareHelper.INVALID_SQUARE)
            {
                PieceType promPt = (lo.Length == 3) ? lo[2] switch {
                    'q' => PieceHelper.QUEEN,  'r' => PieceHelper.ROOK,
                    'b' => PieceHelper.BISHOP, 'n' => PieceHelper.KNIGHT,
                    _   => PieceHelper.NONE } : PieceHelper.NONE;

                var cands = legalArr.Where(mv =>
                    mv.Target == target
                    && PieceHelper.GetPieceType(board.At(mv.Start)) == PieceHelper.PAWN).ToArray();

                if (cands.Length > 0)
                {
                    if (promPt != PieceHelper.NONE)
                    {
                        var pm = cands.FirstOrDefault(mv => MoveFlag.GetPromType(mv.Flag) == promPt, Move.NullMove);
                        return pm.MoveValue != 0 ? pm : cands[0];
                    }
                    return cands.FirstOrDefault(
                        mv => !MoveFlag.IsPromotion(mv.Flag) || MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN,
                        cands[0]);
                }
            }
        }

        // ── Piece SAN  Nf3 / Bxc4 / Qd3 / Nbd7 / N1f3 ───────────────────────
        if (raw.Length >= 2 && "NBRQK".Contains(raw[0]))
        {
            PieceType pt = raw[0] switch {
                'N' => PieceHelper.KNIGHT, 'B' => PieceHelper.BISHOP,
                'R' => PieceHelper.ROOK,   'Q' => PieceHelper.QUEEN,
                'K' => PieceHelper.KING,   _   => PieceHelper.NONE };

            string stripped = raw.Replace("x", "").Replace("X", "");
            if (stripped.Length >= 3 && pt != PieceHelper.NONE)
            {
                Square target = SquareHelper.Parse(stripped[^2..].ToLower());
                if (target != SquareHelper.INVALID_SQUARE)
                {
                    var cands = legalArr.Where(mv =>
                        mv.Target == target
                        && PieceHelper.GetPieceType(board.At(mv.Start)) == pt).ToArray();

                    if (cands.Length == 1) return cands[0];
                    if (cands.Length > 1 && stripped.Length >= 4)
                    {
                        char dis = stripped[1];
                        if (char.IsLetter(dis))
                        {
                            int df = dis - 'a';
                            var fm = cands.FirstOrDefault(mv => SquareHelper.GetFile(mv.Start) == df, Move.NullMove);
                            if (fm.MoveValue != 0) return fm;
                        }
                        else if (char.IsDigit(dis))
                        {
                            int dr = dis - '1';
                            var rm = cands.FirstOrDefault(mv => SquareHelper.GetRank(mv.Start) == dr, Move.NullMove);
                            if (rm.MoveValue != 0) return rm;
                        }
                        return cands[0];
                    }
                    if (cands.Length > 1) return cands[0];
                }
            }
        }

        return null;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    static void Cw(string text, ConsoleColor colour)
    {
        Console.ForegroundColor = colour;
        Console.Write(text);
        Console.ResetColor();
    }
}
