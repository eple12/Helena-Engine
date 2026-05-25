using System.Diagnostics;
using System.Threading;
using H.Core;

namespace H.Engine;

public class Engine
{
    const int INF = Constants.INF;
    Board board;
    TT tt;
    PvTable pv;

    bool useBook = true;

    // Field
    // volatile: these flags are read/written by both the UCI thread and the search thread without a lock
    volatile bool isSearching = false;
    // 0 for false, 1 for true. Used with Interlocked to ensure atomic operations.
    private int _searchRequestedFlag = 0;
    volatile bool cancellationRequested = false;
    SearchRequest lastSearchRequest;
    Action OnSearchComplete;
    Stopwatch searchTimer = new();

    MoveOrdering moveOrdering;
    SEE see;

    // Difficulty
    DifficultyLevel currentDifficulty = DifficultyLevel.MAXIMUM;
    Random rng = new();

    // Play-mode callback: fired with the chosen move just before "bestmove" is printed
    public Action<Move>? OnBestMoveFound;

    // Search vars
    Move bestMove;
    Move bestMoveLastIteration;
    int bestEval;

    ulong numNodesSearched;
    
    public Engine(Board _board)
    {
        board = _board;
        tt = new(_board);

        see = new(board);
        moveOrdering = new(_board, see);
        pv = new();

        OnSearchComplete = () => {};

        Task.Factory.StartNew(SearchThread, TaskCreationOptions.LongRunning);
    }
    
    void Search(int maxDepth)
    {
        if (useBook)
        {
            Move bookMove = TryGetBookMove();
            if (bookMove != Move.NullMove)
            {
                // Fire the callback so PlayMode (or any other subscriber) is unblocked.
                OnBestMoveFound?.Invoke(bookMove);
                System.Console.WriteLine($"bestmove {bookMove.Notation}");
                return;
            }
        }
        IDDFS(maxDepth);
    }
    void ResetField()
    {
        pv.ClearAll();
        moveOrdering.ClearHistory();
        moveOrdering.ClearKillerMoves();

        searchTimer.Reset();
        numNodesSearched = 0;

        bestMove = Move.NullMove;
        bestMoveLastIteration = Move.NullMove;
        bestEval = -INF;
    }
    void EndSearch()
    {
        isSearching = false;
        cancellationRequested = false;
        OnSearchComplete?.Invoke();
        OnSearchComplete = () => {};

        ResetField();
    }

    void IDDFS(int maxDepth)
    {
        MoveList currentLegalMoves = board.MoveGenerator.GenerateMoves();
        if (currentLegalMoves.Length == 0)
        {
            System.Console.WriteLine("No legal moves found.");
            return;
        }

        pv.ClearAll();

        int alpha = -INF;
        int beta = INF;

        int lastEval = -INF;

        maxDepth = Math.Min(maxDepth, Constants.MAX_DEPTH);
        
        searchTimer.Start();

        for (int depth = 1; depth <= maxDepth; depth++)
        {
            if (depth < Constants.AspirationWindowDepth || lastEval == -INF)
            {
                Negamax(depth, 0, alpha, beta);
            }
            else // Aspiration Windows
            {
                int window = Constants.AspirationWindowBase;

                alpha = Math.Max(-INF, lastEval - window);
                beta = Math.Min(INF, lastEval + window);

                int numAspirations = 0;

                while (true)
                {
                    if (cancellationRequested)
                    {
                        break;
                    }

                    ++numAspirations;

                    int eval = Negamax(depth, 0, alpha, beta);
                    window += window >> 1; // + window/2

                    if (alpha >= eval) // Fail low
                    {
                        alpha = Math.Max(-INF, eval - window);
                    }
                    else if (eval >= beta) // Fail high
                    {
                        beta = Math.Min(INF, eval + window);
                    }
                    else
                    {
                        // Search complete
                        break;
                    }

                    if (numAspirations >= Constants.MaxAspirations || Evaluation.IsMateEval(eval))
                    {
                        alpha = -INF;
                        beta = INF;

                        // Window reset to full-width. The next Negamax call will be a full-window search,
                        // after which the while(true) loop will break via the else branch above.
                    }
                }
            }

            bestMoveLastIteration = bestMove;
            lastEval = bestEval;
 
            pv.ClearExceptRoot();
            if (bestMove == Move.NullMove)
            {
                bestMove = currentLegalMoves[0];
            }
            if (pv[0] == Move.NullMove)
            {
                pv[0] = bestMove;
            }

            bool isMate = Evaluation.IsMateEval(bestEval);
            int matePly = isMate ? Evaluation.MateInPly(bestEval): 0;

            string scoreString = !isMate ? $"cp {bestEval}" : $"mate {((bestEval > 0) ? (matePly + 1) / 2 : -matePly / 2)}";
            ulong elapsedMS = Math.Max((ulong) searchTimer.ElapsedMilliseconds, 1);
            System.Console.WriteLine($"info depth {depth} score {scoreString} nodes {numNodesSearched} time {elapsedMS} nps {numNodesSearched * 1000 / elapsedMS} pv {pv.GetRootString()}");

            if (cancellationRequested)
            {
                break;
            }

            if (isMate && (matePly <= depth))
            {
                break;
            }
        }

        searchTimer.Stop();

        Move finalMove = (currentDifficulty == DifficultyLevel.MAXIMUM)
            ? bestMove
            : ApplyDifficulty(bestMove);

        OnBestMoveFound?.Invoke(finalMove);
        System.Console.WriteLine($"bestmove {finalMove.Notation}");
    }

    int Negamax(int depth, int plyFromRoot, int alpha, int beta)
    {
        ++numNodesSearched;
        if (cancellationRequested)
        {
            return 0;
        }

        if (plyFromRoot >= Constants.MAX_DEPTH)
        {
            return Evaluation.Eval();
        }

        bool isRoot = plyFromRoot == 0;
        bool isPv = beta - alpha > 1;
        byte evalType = TT.Alpha;

        int pvIndex = PvTable.Indexes[plyFromRoot];
        int nextPvIndex = PvTable.Indexes[plyFromRoot + 1];
        pv[pvIndex] = Move.NullMove;

        // Invalidate the PV for the current ply right away.
        // This ensures that if we have an early exit (TT hit, draw, etc.),
        // the parent node will not copy stale PV data from this ply.
        
        // pv.ClearFrom(pvIndex);

        if (!isRoot)
        {
            if (board.IsFiftyMoveDraw() || board.IsRepetition() || board.IsInsufficientMaterial())
            {
                pv.ClearFrom(nextPvIndex);
                return 0;
            }
        }
        

        Move ttMove = Move.NullMove;
        if (!isRoot)
        {
            int ttVal = tt.LookupEval(depth, plyFromRoot, alpha, beta);
            if (ttVal != TT.LookupFailed)
            {
                ttMove = tt.GetStoredMove();

                if (!isPv)
                {
                    return ttVal;    
                }
            }
        }

        if (depth <= 0)
        {
            return QuiescenceSearch(alpha, beta);
        }

        // MoveList moves = board.MoveGenerator.GenerateMoves();
        MoveList moves = stackalloc Move[Constants.MAX_MOVES];
        board.MoveGenerator.GenerateMoves(ref moves, capturesOnly: false);
        bool inCheck = board.InCheck();

        if (moves.Length == 0)
        {
            pv.ClearFrom(nextPvIndex);

            if (inCheck)
            {
                return -Evaluation.MateEval(plyFromRoot);
            }
            return 0;
        }

        // SEE Bad Capture start/end index
        // If there is no bad capture move, returns (-1, -1)
        // In that case, start <= i <= end never satisfies
        (int seePruningStart, int seePruningEnd) = moveOrdering.GetOrderedMoves(ref moves, isRoot ? bestMoveLastIteration : ttMove, false, plyFromRoot);
        Move bestMoveThisPosition = moves[0];

        if (inCheck && depth < Constants.MAX_DEPTH)
        {
            depth++;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            bool isCapture = MoveFlag.IsCapture(moves[i].Flag);
            board.MakeMove(moves[i]);
            
            int score = 0;
            int reduction = 0;
            bool givesCheck = board.InCheck();

            if (
                depth >= Constants.LMR_MinDepth && i >= (isPv ? Constants.LMR_MinFullSearchMoves : Constants.LMR_MinFullSearchMoves - 1)
                && !isCapture
            )
            {
                reduction = Constants.LMR_Reductions[depth][i];

                if (isPv)
                {
                    --reduction;
                }
                if (givesCheck)
                {
                    --reduction;
                }

                reduction = Math.Clamp(reduction, 0, depth - 1);
            }

            // SEE Reduction
            // Improvements: Don't copy move scores, maybe just return the range of bad capture moves
            if (
                !inCheck && i >= seePruningStart && i <= seePruningEnd
            )
            {
                reduction += Constants.SEE_BadCaptureReduction;
                reduction = Math.Clamp(reduction, 0, depth - 1);
            }

            score = -Negamax(depth - 1 - reduction, plyFromRoot+ 1, -alpha - 1, -alpha);

            if (score > alpha && reduction > 0) {
                score = -Negamax(depth - 1, plyFromRoot + 1, -alpha - 1, -alpha);
            }

            if (score > alpha && score < beta)
            {
                score = -Negamax(depth - 1, plyFromRoot + 1, -beta, -alpha);
            }
            
            board.UnmakeMove(moves[i]);

            if (cancellationRequested)
            {
                return 0;
            }

            if (score >= beta)
            {
                tt.StoreEval(depth, plyFromRoot, beta, TT.Beta, moves[i]);

                if (!isCapture)
                {
                    if (plyFromRoot < Constants.MaxKillerPly)
                    {
                        moveOrdering.KillerMoves[plyFromRoot].Add(moves[i]);
                    }

                    int historyScore = depth * depth;
                    moveOrdering.History[board.State.SideToMove ? 0 : 1, moves[i].Start, moves[i].Target] += historyScore;
                }

                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
                bestMoveThisPosition = moves[i];
                evalType = TT.Exact;

                // Record PV
                // int pvIndex = PvTable.Indexes[plyFromRoot];
                if (isPv)
                {
                    pv[pvIndex] = moves[i];
                    pv.CopyFrom(pvIndex + 1, nextPvIndex, Constants.MAX_DEPTH - plyFromRoot - 1);
                }
                

                if (isRoot)
                {
                    // Requires move ordering
                    bestMove = moves[i];
                    bestEval = alpha;
                }
            }
        }

        tt.StoreEval(depth, plyFromRoot, alpha, evalType, bestMoveThisPosition);

        return alpha;
    }

    int QuiescenceSearch(int alpha, int beta)
    {
        ++numNodesSearched;
        int eval = Evaluation.Eval();

        if (eval >= beta)
        {
            return beta;
        }
        if (eval > alpha)
        {
            alpha = eval;
        }

        // MoveList moves = board.MoveGenerator.GenerateMoves(capturesOnly: true);
        MoveList moves = stackalloc Move[128];
        board.MoveGenerator.GenerateMoves(ref moves, capturesOnly: true);
        moveOrdering.GetOrderedMoves(ref moves, Move.NullMove, true, 0);

        for (int i = 0; i < moves.Length; i++)
        {
            board.MakeMove(moves[i]);
            int score = -QuiescenceSearch(-beta, -alpha);
            board.UnmakeMove(moves[i]);

            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                alpha = score;
            }
        }

        return alpha;
    }

    Move TryGetBookMove()
    {
        return Book.Book.GetRandomMove(board.State.Key);
    }

    // ── Difficulty ────────────────────────────────────────────────────────────

    /// <summary>
    /// Probabilistic move selection based on the current difficulty level.
    ///
    /// Algorithm:
    ///   1. Score every legal root move with a quiescence search.
    ///   2. Sort moves best-to-worst by that score.
    ///   3. Restrict candidates to the top (MaxN + 1) moves.
    ///   4. Assign softmax weights: weight[i] = exp(−diff[i] / Temperature),
    ///      where diff[i] = topScore − score[i] ≥ 0.
    ///   5. Sample one candidate according to those weights.
    ///
    /// Moves beyond rank MaxN always have weight 0, preventing truly catastrophic
    /// blunders even at the lowest difficulty.
    /// Large eval gaps naturally suppress bad candidates without special-casing.
    /// </summary>
    Move ApplyDifficulty(Move mainSearchBestMove)
    {
        DifficultyConfig config = DifficultySettings.Get(currentDifficulty);
        int maxN = config.MaxN;
        double temp = config.Temperature;

        // Collect all legal moves from the current root position
        MoveList legalMoves = board.MoveGenerator.GenerateMoves();
        int n = legalMoves.Length;

        if (n <= 1) return mainSearchBestMove;

        // Score every legal move with a quiescence search
        var scored = new (Move move, int score)[n];
        for (int i = 0; i < n; i++)
        {
            board.MakeMove(legalMoves[i]);
            int qs = -QuiescenceSearch(-INF, INF);
            board.UnmakeMove(legalMoves[i]);
            scored[i] = (legalMoves[i], qs);
        }

        // Sort descending by score (best move first)
        Array.Sort(scored, (a, b) => b.score.CompareTo(a.score));

        // Only the top (maxN + 1) moves are eligible
        int numCandidates = Math.Min(maxN + 1, n);
        int topScore = scored[0].score;

        // Softmax weights
        double[] weights = new double[numCandidates];
        double totalWeight = 0.0;
        for (int i = 0; i < numCandidates; i++)
        {
            double diff = Math.Max(0.0, (double)(topScore - scored[i].score));
            weights[i] = Math.Exp(-diff / temp);
            totalWeight += weights[i];
        }

        // Weighted random selection
        double r = rng.NextDouble() * totalWeight;
        double cumulative = 0.0;
        for (int i = 0; i < numCandidates; i++)
        {
            cumulative += weights[i];
            if (r <= cumulative)
            {
                if (i > 0)
                {
                    System.Console.WriteLine(
                        $"info string [{config.Name}] rank {i + 1} selected: {scored[i].move.Notation} " +
                        $"(eval diff: {topScore - scored[i].score} cp)");
                }
                return scored[i].move;
            }
        }

        // Fallback – should not be reached in practice
        return scored[0].move;
    }

    public void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        DifficultyConfig config = DifficultySettings.Get(level);
        System.Console.WriteLine(
            $"info string Difficulty set to [{(int)level}] {config.Name} – {config.Description}");
    }

    public DifficultyLevel GetDifficulty() => currentDifficulty;

    public void ToggleBook()
    {
        useBook = !useBook;
    }
    public bool GetBookToggle()
    {
        return useBook;
    }
    public bool IsSearching()
    {
        return isSearching;
    }
    public void RequestSearch(int maxDepth, Action? onSearchComplete = null)
    {
        // This request might overwrite a previous one if it hasn't been picked up by the search thread yet.
        // This is usually fine as we typically only care about the latest search request.
        lastSearchRequest = new(maxDepth, onSearchComplete);
        Interlocked.Exchange(ref _searchRequestedFlag, 1);
    }
    public void CancelSearch()
    {
        cancellationRequested = true;
    }

    // Handles UCI "ucinewgame": clears TT and resets search history
    // to prevent stale entries from a previous game from polluting the new one.
    public void NewGame()
    {
        tt.Clear();
        moveOrdering.ClearHistory();
        moveOrdering.ClearKillerMoves();
        pv.ClearAll();
    }

    // Multithreading
    void SearchThread()
    {
        while (true)
        {
            // Atomically check if a search has been requested and consume the flag.
            // This prevents a race condition where the loop could spin and start the same search multiple times.
            if (Interlocked.CompareExchange(ref _searchRequestedFlag, 0, 1) == 1)
            {
                // A request has been made and we have consumed the flag.
                // We don't start a new search if one is already running.
                if (!isSearching)
                {
                    StartSearchThreaded(lastSearchRequest.Depth, lastSearchRequest.OnSearchComplete);
                }
            }

            // Sleep for a short duration to prevent this thread from consuming a full CPU core.
            Thread.Sleep(10);
        }
    }
    void StartSearchThreaded(int maxDepth, Action? onSearchComplete = null)
    {
        Interlocked.Exchange(ref _searchRequestedFlag, 0);
        isSearching = true;

        if (onSearchComplete != null)
        {
            OnSearchComplete = onSearchComplete;
        }

        Search(maxDepth);
        EndSearch();
    }
}


struct SearchRequest(int maxDepth, Action? onSearchComplete)
{
    public int Depth => maxDepth;
    public Action? OnSearchComplete => onSearchComplete;
}
