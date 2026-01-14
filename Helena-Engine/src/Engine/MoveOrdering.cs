namespace H.Engine;

using H.Core;

public class MoveOrdering
{
    Board board;
    int[] moveScores;

    public const int HashMoveScore = 2_097_152;
    // public const int QueenPromotionCaptureBaseScore = GoodCaptureBaseScore + PromotionMoveScore;
    public const int GoodCaptureBaseScore = 1_048_576;
    public const int KillerMoveValue = 524_288;
    public const int PromotionMoveScore = 32_768;
    public const int BadCaptureBaseScore = 16_384;

    ulong enemyPawnAttackMap;
    ulong enemyAttackMapNoPawn;
    ulong enemyAttackMap;

    public int[,,] History;
    public Killers[] KillerMoves;

    int color;

    public MoveOrdering(Board _board)
    {
        board = _board;
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

        int score = 0;
        ushort flag = move.Flag;
        
        PieceType movingPieceType = PieceHelper.GetPieceType(board.At(move.Start));
        PieceType capturedPieceType = PieceHelper.GetPieceType(board.At(move.Target));

        bool isCapture = MoveFlag.IsCapture(flag);

        // MVV LVA
        if (isCapture)
        {
            int captureDelta = Evaluation.GetMaterialValue(capturedPieceType) - Evaluation.GetMaterialValue(movingPieceType);
            
            // Requires SEE for better insight
            if (enemyAttackMap.Contains(move.Target))
            {
                score += (captureDelta >= 0 ? GoodCaptureBaseScore : BadCaptureBaseScore) + captureDelta;
            }
            else
            {
                score += GoodCaptureBaseScore + captureDelta;
            }
        }
        if (MoveFlag.IsPromotion(flag))
        {
            score += PromotionMoveScore;

            PieceType promType = MoveFlag.GetPromType(flag);
            int promValue = Evaluation.GetMaterialValue(promType);

            score += promValue;
        }
        else if (!isCapture)
        {
            // PSQT
            // int color = board.State.SideToMove ? 0 : 1;
            int psqtDiff = Evaluation.GetPsqtScore(movingPieceType, color, move.Target) - Evaluation.GetPsqtScore(movingPieceType, color, move.Start);
            score += psqtDiff;

            // Enemy Attacks
            if (enemyPawnAttackMap.Contains(move.Target))
            {
                score -= 200;
            }
            else if (enemyAttackMapNoPawn.Contains(move.Target))
            {
                score -= 100;
            }

            // Killer
            // History

            if (!inQSearch)
            {
                bool isKiller = ply < Constants.MaxKillerPly && KillerMoves[ply].Match(move);
                if (isKiller)
                {
                    score += KillerMoveValue;
                }

                score += History[color, move.Start, move.Target] * 100;
            }
        }
        
        return score;
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