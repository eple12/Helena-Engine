namespace H.Program;

using System.Text;
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
///   hint     show engine's top suggestions
///   undo     take back the last player + engine move pair
///   resign   forfeit the game
///   quit     exit the game and return to the UCI prompt
/// </summary>
public static class PlayMode
{
    // ── Console colours ───────────────────────────────────────────────────────

    const ConsoleColor LightSquareBg  = ConsoleColor.DarkYellow;
    const ConsoleColor DarkSquareBg   = ConsoleColor.DarkCyan;
    const ConsoleColor HighlightBg    = ConsoleColor.DarkGreen;
    const ConsoleColor WhitePieceFg   = ConsoleColor.White;
    const ConsoleColor BlackPieceFg   = ConsoleColor.Black;
    const ConsoleColor EmptyDotFg     = ConsoleColor.DarkGray;

    // Unicode chess pieces  [0=none, 1=P, 2=N, 3=B, 4=R, 5=Q, 6=K]
    static readonly string[] WhitePieceGlyphs = ["·", "♙", "♘", "♗", "♖", "♕", "♔"];
    static readonly string[] BlackPieceGlyphs = ["·", "♟", "♞", "♝", "♜", "♛", "♚"];

    // ── Entry point ────────────────────────────────────────────────────────────

    public static bool Run(bool playerIsWhite, int moveTimeMs, int maxDepth)
    {
        Console.OutputEncoding = Encoding.UTF8;

        Board  board  = Main.MainBoard;
        EnginePlayer engine = Main.MainEnginePlayer;

        board.LoadPositionFromFEN(UCI.STARTPOS_FEN);
        engine.NewGame();

        DifficultyConfig diffCfg = DifficultySettings.Get(engine.GetDifficulty());

        // Move history for undo (stores full Move objects)
        var history = new List<Move>();

        Move  lastEngineMove = Move.NullMove;
        bool  wantQuit       = false;

        PrintWelcome(playerIsWhite, diffCfg, moveTimeMs, maxDepth);

        // ── Main game loop ────────────────────────────────────────────────────

        while (true)
        {
            PrintBoard(board, playerIsWhite, lastEngineMove);

            // Game-end check
            MoveList legal = board.MoveGenerator.GenerateMoves();
            if (legal.Length == 0)
            {
                if (board.InCheck())
                    PrintResult(board.State.SideToMove ? "Black wins by checkmate! ♟" : "White wins by checkmate! ♙");
                else
                    PrintResult("Stalemate – it's a draw.");
                break;
            }
            if (board.IsFiftyMoveDraw())  { PrintResult("Draw by 50-move rule."); break; }
            if (board.IsRepetition())     { PrintResult("Draw by repetition."); break; }
            if (board.IsInsufficientMaterial()) { PrintResult("Draw by insufficient material."); break; }

            bool isPlayerTurn = board.State.SideToMove == playerIsWhite;

            if (isPlayerTurn)
            {
                // ── Player's turn ─────────────────────────────────────────────

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
                        PrintResult(playerIsWhite ? "You resigned. Black wins." : "You resigned. White wins.");
                        goto GameOver;
                    }
                    if (lo is "hint" or "h")
                    {
                        ShowHint(engine, board, moveTimeMs, maxDepth);
                        continue;
                    }
                    if (lo == "undo")
                    {
                        if (history.Count < 2)
                        {
                            Cw("  Nothing to undo.", ConsoleColor.DarkYellow); Console.WriteLine();
                            continue;
                        }
                        // Unmake engine move, then player move
                        board.UnmakeMove(history[^1]); history.RemoveAt(history.Count - 1);
                        board.UnmakeMove(history[^1]); history.RemoveAt(history.Count - 1);
                        lastEngineMove = history.Count >= 2 ? history[^1] : Move.NullMove;
                        Cw("  Undo successful.", ConsoleColor.Green); Console.WriteLine();
                        break;
                    }

                    Move? parsed = TryParseMoveInput(raw, board);
                    if (parsed is null || parsed.Value.MoveValue == 0)
                    {
                        Cw($"  Invalid move: '{raw}'. Try e2e4, e4, Nf3, O-O …", ConsoleColor.Red);
                        Console.WriteLine();
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
                // ── Engine's turn ─────────────────────────────────────────────

                string side = playerIsWhite ? "Black (Engine)" : "White (Engine)";
                Console.Write($"\n  {side} is thinking");
                Console.Out.Flush();

                Move engineMove = Move.NullMove;
                using var done = new System.Threading.ManualResetEventSlim(false);

                engine.SetBestMoveCallback(m =>
                {
                    engineMove = m;
                    done.Set();
                });

                engine.Search(maxDepth, moveTimeMs);
                done.Wait();
                engine.SetBestMoveCallback(null);

                Console.WriteLine();

                if (engineMove.MoveValue == 0)
                {
                    Cw("  Engine returned no move.", ConsoleColor.Red);
                    Console.WriteLine();
                    break;
                }

                board.MakeMove(engineMove);
                history.Add(engineMove);
                lastEngineMove = engineMove;

                string moveDesc = FormatMoveDescription(engineMove, board);
                Console.Write($"  Engine played: ");
                Cw(moveDesc, ConsoleColor.Cyan);
                Console.WriteLine();
            }
        }

        GameOver:
        Console.WriteLine();
        Cw("  Returned to UCI mode. Type 'play' to start a new game.", ConsoleColor.DarkGray);
        Console.WriteLine("\n");
        return wantQuit;   // true → Program should exit
    }

    // ── Board rendering ────────────────────────────────────────────────────────

    static void PrintBoard(Board board, bool whitePov, Move lastEngineMove)
    {
        Console.WriteLine();

        // File labels (top)
        Console.Write("     ");
        for (int fi = 0; fi < 8; fi++)
        {
            int file = whitePov ? fi : 7 - fi;
            Console.Write($" {(char)('a' + file)}  ");
        }
        Console.WriteLine();
        Console.WriteLine("   ┌────────────────────────────────┐");

        for (int ri = 0; ri < 8; ri++)
        {
            int rank = whitePov ? 7 - ri : ri;
            Console.Write($" {rank + 1} │");

            for (int fi = 0; fi < 8; fi++)
            {
                int file = whitePov ? fi : 7 - fi;
                Square sq = SquareHelper.GetSquare(file, rank);
                Piece  p  = board.At(sq);

                bool isLight = (file + rank) % 2 == 1;
                bool isHigh  = lastEngineMove.MoveValue != 0
                    && (sq == lastEngineMove.Start || sq == lastEngineMove.Target);

                Console.BackgroundColor = isHigh  ? HighlightBg
                                        : isLight ? LightSquareBg
                                                  : DarkSquareBg;

                if (p == PieceHelper.NONE)
                {
                    Console.ForegroundColor = EmptyDotFg;
                    Console.Write(" · ");
                }
                else
                {
                    bool isWhite = PieceHelper.IsColor(p, PieceHelper.WHITE);
                    Console.ForegroundColor = isWhite ? WhitePieceFg : BlackPieceFg;
                    string glyph = isWhite
                        ? WhitePieceGlyphs[PieceHelper.GetPieceType(p)]
                        : BlackPieceGlyphs[PieceHelper.GetPieceType(p)];
                    Console.Write($" {glyph} ");
                }

                Console.ResetColor();
                Console.Write("│");
            }

            Console.ResetColor();
            Console.WriteLine($" {rank + 1}");

            if (ri < 7)
                Console.WriteLine("   ├────────────────────────────────┤");
        }

        Console.WriteLine("   └────────────────────────────────┘");
        Console.Write("     ");
        for (int fi = 0; fi < 8; fi++)
        {
            int file = whitePov ? fi : 7 - fi;
            Console.Write($" {(char)('a' + file)}  ");
        }
        Console.WriteLine("\n");
    }

    // ── Status / info helpers ──────────────────────────────────────────────────

    static void PrintWelcome(bool playerIsWhite, DifficultyConfig diff, int moveTimeMs, int depth)
    {
        Console.WriteLine();
        Cw("  ╔════════════════════════════════════════╗", ConsoleColor.DarkCyan);
        Console.WriteLine();
        Cw("  ║         Helena-Engine  Play Mode        ║", ConsoleColor.DarkCyan);
        Console.WriteLine();
        Cw("  ╚════════════════════════════════════════╝", ConsoleColor.DarkCyan);
        Console.WriteLine();
        Console.WriteLine($"  You play  : {(playerIsWhite ? "White ♙" : "Black ♟")}");
        Console.WriteLine($"  Difficulty: [{(int)diff.Level}] {diff.Name}  –  {diff.Description}");
        Console.WriteLine($"  Think time: {moveTimeMs / 1000.0:F1} s/move  |  Max depth: {(depth >= Constants.MAX_DEPTH ? "unlimited" : depth.ToString())}");
        Console.WriteLine();
        Cw("  Commands: resign  undo  hint  quit", ConsoleColor.DarkGray);
        Console.WriteLine("\n");
    }

    static void PrintTurnHeader(Board board, bool playerIsWhite)
    {
        string side = playerIsWhite ? "White ♙" : "Black ♟";
        bool inCheck = board.InCheck();
        Console.Write($"\n  Your turn ({side})");
        if (inCheck) { Console.Write("  "); Cw("CHECK!", ConsoleColor.Red); }
        Console.WriteLine();
    }

    static void PrintLegalMoveList(MoveList legal)
    {
        Console.Write($"  Legal moves ({legal.Length}): ");
        var arr = legal.ToArray();
        // Group: captures first, then quiet
        var captures = arr.Where(m => MoveFlag.IsCapture(m.Flag)).Select(m => m.Notation);
        var quiet    = arr.Where(m => !MoveFlag.IsCapture(m.Flag)).Select(m => m.Notation);
        string line  = string.Join(" ", captures.Concat(quiet));
        // Wrap at ~80 chars
        const int wrap = 72;
        int col = 18;
        foreach (string tok in line.Split(' '))
        {
            if (col + tok.Length + 1 > wrap) { Console.Write("\n              "); col = 14; }
            Console.Write(tok + " ");
            col += tok.Length + 1;
        }
        Console.WriteLine();
    }

    static void PrintResult(string message)
    {
        Console.WriteLine();
        Cw($"  ══ {message} ══", ConsoleColor.Yellow);
        Console.WriteLine("\n");
    }

    // ── Hint ──────────────────────────────────────────────────────────────────

    static void ShowHint(EnginePlayer engine, Board board, int moveTimeMs, int depth)
    {
        Console.Write("\n  Calculating hint");
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
            Cw(best.Notation, ConsoleColor.Green);
            Console.WriteLine();
        }
    }

    // ── Move description ──────────────────────────────────────────────────────

    static string FormatMoveDescription(Move m, Board board)
    {
        // Board is AFTER the move was made; unmake to inspect starting piece
        board.UnmakeMove(m);
        Piece  p    = board.At(m.Start);
        string name = PieceHelper.GetPieceType(p) switch
        {
            PieceHelper.PAWN   => "Pawn",
            PieceHelper.KNIGHT => "Knight",
            PieceHelper.BISHOP => "Bishop",
            PieceHelper.ROOK   => "Rook",
            PieceHelper.QUEEN  => "Queen",
            PieceHelper.KING   => "King",
            _ => "?"
        };
        string desc = MoveFlag.IsCastling(m.Flag)   ? (m.Flag == MoveFlag.KCastling ? "O-O (Kingside castle)" : "O-O-O (Queenside castle)")
                    : MoveFlag.IsCapture(m.Flag)      ? $"{name} {m.Notation[..2]} × {m.Notation[2..4]}"
                    : MoveFlag.IsPromotion(m.Flag)    ? $"Pawn → {m.Notation}"
                    : $"{name} {m.Notation}";
        board.MakeMove(m);   // restore
        return desc;
    }

    // ── Move input parsing ────────────────────────────────────────────────────

    /// <summary>
    /// Accepts:
    ///   UCI       e2e4  e7e8q
    ///   Short pawn  e4  exd5  e8q  (auto-queen if rank 8 and no piece specified)
    ///   Piece SAN   Nf3  Bxc4  Qd3  Rxe1  Kd2
    ///   Castling    O-O  O-O-O  0-0  0-0-0
    /// </summary>
    static Move? TryParseMoveInput(string raw, Board board)
    {
        MoveList legal = board.MoveGenerator.GenerateMoves();
        Move[] legalArr = legal.ToArray();
        string lo = raw.ToLower().Replace("×", "x").Replace("–","-");

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

        // ── UCI (e2e4 / e7e8q) ────────────────────────────────────────────────
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
                    PieceType pt = lo[4] switch { 'q'=>PieceHelper.QUEEN,'r'=>PieceHelper.ROOK,
                                                  'b'=>PieceHelper.BISHOP,'n'=>PieceHelper.KNIGHT, _=>PieceHelper.NONE };
                    if (pt != PieceHelper.NONE)
                    {
                        var pm = legalArr.FirstOrDefault(
                            mv => mv.Start == start && mv.Target == target && MoveFlag.GetPromType(mv.Flag) == pt, Move.NullMove);
                        return pm.MoveValue != 0 ? pm : null;
                    }
                }
                var m = legalArr.FirstOrDefault(mv => mv.Start == start && mv.Target == target, Move.NullMove);
                // If multiple (prom), pick queen
                if (m.MoveValue == 0) return null;
                if (MoveFlag.IsPromotion(m.Flag) && MoveFlag.GetPromType(m.Flag) != PieceHelper.QUEEN)
                {
                    var qm = legalArr.FirstOrDefault(
                        mv => mv.Start == start && mv.Target == target && MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN, m);
                    return qm;
                }
                return m;
            }
        }

        // ── Pawn captures: exd5, bxc3 ─────────────────────────────────────────
        if (lo.Length >= 4 && lo[1] == 'x')
        {
            string targetStr = lo[2..4];
            int srcFile = lo[0] - 'a';
            Square target = SquareHelper.Parse(targetStr);
            if (srcFile >= 0 && srcFile <= 7 && target != SquareHelper.INVALID_SQUARE)
            {
                var candidates = legalArr.Where(mv =>
                    mv.Target == target
                    && SquareHelper.GetFile(mv.Start) == srcFile
                    && PieceHelper.GetPieceType(board.At(mv.Start)) == PieceHelper.PAWN
                    && MoveFlag.IsCapture(mv.Flag)).ToArray();
                if (candidates.Length > 0)
                {
                    // Handle promotion capture e.g. exd8q
                    if (lo.Length == 5)
                    {
                        PieceType pt = lo[4] switch { 'q'=>PieceHelper.QUEEN,'r'=>PieceHelper.ROOK,
                                                      'b'=>PieceHelper.BISHOP,'n'=>PieceHelper.KNIGHT, _=>PieceHelper.QUEEN };
                        var pm = candidates.FirstOrDefault(mv => MoveFlag.GetPromType(mv.Flag) == pt, candidates[0]);
                        return pm;
                    }
                    // Default: pick queen promotion if available
                    var qm = candidates.FirstOrDefault(mv => !MoveFlag.IsPromotion(mv.Flag)
                        || MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN, candidates[0]);
                    return qm;
                }
            }
        }

        // ── Short pawn move: e4, e8, e8q ──────────────────────────────────────
        if (lo.Length >= 2 && char.IsLetter(lo[0]) && char.IsDigit(lo[1]))
        {
            Square target = SquareHelper.Parse(lo[0..2]);
            if (target != SquareHelper.INVALID_SQUARE)
            {
                PieceType promPt = (lo.Length == 3) ? lo[2] switch {
                    'q'=>PieceHelper.QUEEN,'r'=>PieceHelper.ROOK,
                    'b'=>PieceHelper.BISHOP,'n'=>PieceHelper.KNIGHT, _=>PieceHelper.NONE } : PieceHelper.NONE;

                var candidates = legalArr.Where(mv =>
                    mv.Target == target
                    && PieceHelper.GetPieceType(board.At(mv.Start)) == PieceHelper.PAWN).ToArray();

                if (candidates.Length > 0)
                {
                    if (promPt != PieceHelper.NONE)
                    {
                        var pm = candidates.FirstOrDefault(mv => MoveFlag.GetPromType(mv.Flag) == promPt, Move.NullMove);
                        return pm.MoveValue != 0 ? pm : candidates[0];
                    }
                    // Auto-queen promotion
                    var qm = candidates.FirstOrDefault(
                        mv => !MoveFlag.IsPromotion(mv.Flag) || MoveFlag.GetPromType(mv.Flag) == PieceHelper.QUEEN,
                        candidates[0]);
                    return qm;
                }
            }
        }

        // ── Piece SAN: Nf3, Bxc4, Qd3, Rxe1, Kd2, Nbd7 ─────────────────────
        if (raw.Length >= 2 && "NBRQK".Contains(raw[0]))
        {
            PieceType pieceType = raw[0] switch {
                'N'=>PieceHelper.KNIGHT,'B'=>PieceHelper.BISHOP,'R'=>PieceHelper.ROOK,
                'Q'=>PieceHelper.QUEEN,'K'=>PieceHelper.KING, _=>PieceHelper.NONE };

            // Strip 'x' (capture marker) and get target square (last 2 chars)
            string stripped = raw.Replace("x","").Replace("X","");
            if (stripped.Length >= 3)
            {
                string targetStr = stripped[^2..].ToLower();
                Square target = SquareHelper.Parse(targetStr);
                if (target != SquareHelper.INVALID_SQUARE && pieceType != PieceHelper.NONE)
                {
                    var candidates = legalArr.Where(mv =>
                        mv.Target == target
                        && PieceHelper.GetPieceType(board.At(mv.Start)) == pieceType).ToArray();

                    if (candidates.Length == 1) return candidates[0];
                    if (candidates.Length > 1)
                    {
                        // Disambiguation: Nbd7 → file 'b', N1f3 → rank '1'
                        if (stripped.Length >= 4)
                        {
                            char dis = stripped[1];
                            if (char.IsLetter(dis))
                            {
                                int disFile = dis - 'a';
                                var fm = candidates.FirstOrDefault(mv => SquareHelper.GetFile(mv.Start) == disFile, Move.NullMove);
                                if (fm.MoveValue != 0) return fm;
                            }
                            else if (char.IsDigit(dis))
                            {
                                int disRank = dis - '1';
                                var rm = candidates.FirstOrDefault(mv => SquareHelper.GetRank(mv.Start) == disRank, Move.NullMove);
                                if (rm.MoveValue != 0) return rm;
                            }
                        }
                        return candidates[0];
                    }
                }
            }
        }

        return null;
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    /// <summary>Write text in the given colour, then reset.</summary>
    static void Cw(string text, ConsoleColor colour)
    {
        Console.ForegroundColor = colour;
        Console.Write(text);
        Console.ResetColor();
    }
}
