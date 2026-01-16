namespace H.Engine;

using H.Core;

public class MoveOrdering
{
    Board board;
    SEE see;
    int[] moveScores;

    static readonly int[] MaterialValues = EvaluationConstants.AbsoluteMaterial;

    public const int HashMoveScore = 2_097_152;
    public const int QueenPromotionCaptureBaseScore = GoodCaptureBaseScore + PromotionMoveScore;
    public const int GoodCaptureBaseScore = 1_048_576;
    public const int KillerMoveValue = 524_288;
    public const int PromotionMoveScore = 32_768;
    public const int BadCaptureBaseScore = 16_384;

    // Negative value to make sure history moves doesn't reach other important moves
    public const int BaseMoveScore = int.MinValue / 2;

    ulong enemyPawnAttackMap;
    ulong enemyAttackMapNoPawn;
    ulong enemyAttackMap;

    public int[,,] History;
    public Killers[] KillerMoves;

    int color;

    public MoveOrdering(Board _board, SEE _see)
    {
        board = _board;
        see = _see;
        moveScores = new int[Constants.MAX_MOVES];

        History = new int[2, 64, 64];
        KillerMoves = new Killers[Constants.MaxKillerPly];
    }

    public MoveList GetOrderedMoves(ref MoveList moves, Move lastIteration, bool inQSearch, int ply)
    {
        // We still have these values in MoveGen since we call MoveOrdering right after MoveGen
        enemyPawnAttackMap = board.MoveGenerator.EnemyPawnAttackMap();
        enemyAttackMapNoPawn = board.MoveGenerator.EnemyAttackMapNoPawn();
        enemyAttackMap = board.MoveGenerator.EnemyAttackMap();

        color = board.State.SideToMove ? 0 : 1;

        GetScores(ref moves, lastIteration, inQSearch, ply);
        Quicksort(ref moves, moveScores, 0, moves.Length - 1);

        return moves;
    }

    void GetScores(ref MoveList moves, Move lastIteration, bool inQSearch, int ply)
    {
        for (int i = 0; i < moves.Length; i++)
        {
            Move move = moves[i];

            moveScores[i] = ScoreMove(move, lastIteration, inQSearch, ply);
        }
    }

    int ScoreMove(Move move, Move hash, bool inQSearch, int ply)
    {
        if (move == hash)
        {
            return HashMoveScore;
        }

        Piece movingPieceType = PieceHelper.GetPieceType(board.At(move.Start));
        Piece capturedPieceType = PieceHelper.GetPieceType(board.At(move.Target));

        int capturePieceValue = MaterialValues[capturedPieceType];

        // If Queen promotion
        if (move.Flag == MoveFlag.QPromCapture)
        {
            return QueenPromotionCaptureBaseScore + capturePieceValue;
        }
        if (move.Flag == MoveFlag.QProm)
        {
            return PromotionMoveScore + (see.HasPositiveScore(move) ? GoodCaptureBaseScore : BadCaptureBaseScore);
        }

        if (MoveFlag.IsCapture(move.Flag))
        {
            int baseCapture = (move.Flag == MoveFlag.EP || MoveFlag.IsPromotion(move.Flag) || see.IsGoodCapture(move)) ? GoodCaptureBaseScore : BadCaptureBaseScore;

            return baseCapture + MVVLVA[movingPieceType][capturedPieceType];
        }

        if (MoveFlag.IsPromotion(move.Flag))
        {
            return PromotionMoveScore;
        }

        if (!inQSearch)
        {
            bool isKiller = ply < Constants.MaxKillerPly && KillerMoves[ply].Match(move);
            return BaseMoveScore + (isKiller ? KillerMoveValue : 0) + History[board.State.SideToMove ? 0 : 1, move.Start, move.Target] * 100 + PSQT.ReadTableFromPiece(movingPieceType, move.Target, board.State.SideToMove);
        }

        return BaseMoveScore + PSQT.ReadTableFromPiece(movingPieceType, move.Target, board.State.SideToMove);
    }

    public void ClearHistory()
    {
        History = new int[2, 64, 64];
    }

    public void ClearKillerMoves()
    {
        KillerMoves = new Killers[Constants.MaxKillerPly];
    }

    public static void Quicksort(ref MoveList values, int[] scores, int low, int high)
    {
        if (low < high)
        {
            int pivotIndex = Partition(ref values, scores, low, high);
            Quicksort(ref values, scores, low, pivotIndex - 1);
            Quicksort(ref values, scores, pivotIndex + 1, high);
        }
    }

    static int Partition(ref MoveList values, int[] scores, int low, int high)
    {
        int pivotScore = scores[high];
        int i = low - 1;

        for (int j = low; j <= high - 1; j++)
        {
            if (scores[j] > pivotScore)
            {
                i++;
                (values[i], values[j]) = (values[j], values[i]);
                (scores[i], scores[j]) = (scores[j], scores[i]);
            }
        }
        (values[i + 1], values[high]) = (values[high], values[i + 1]);
        (scores[i + 1], scores[high]) = (scores[high], scores[i + 1]);

        return i + 1;
    }

    // [Moving PieceType][Captured PieceType]
    static readonly int[][] MVVLVA = [
    //    -     P     N     B     R      Q 
        [ 0,    0,    0,    0,    0,     0 ], // NONE
        [ 0, 1500, 4000, 4500, 5500, 11500 ], // PAWN
        [ 0, 1400, 3900, 4400, 5400, 11400 ], // KNIGHT
        [ 0, 1300, 3800, 4300, 5300, 11300 ], // BISHOP
        [ 0, 1200, 3700, 4200, 5200, 11200 ], // ROOK
        [ 0, 1100, 3600, 4100, 5100, 11100 ], // QUEEN
        [ 0, 1000, 3500, 4000, 5000, 11000 ]  // KING
    ];
}

public struct Killers
{
    public Move moveA;
    public Move moveB;

    public void Add(Move move)
    {
        if (move != moveA)
        {
            moveB = moveA;
            moveA = move;
        }
    }

    public bool Match(Move move) => move == moveA || move == moveB;
}