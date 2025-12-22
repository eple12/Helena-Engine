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

    public MoveOrdering(Board _board)
    {
        board = _board;
        moveScores = new int[Constants.MAX_MOVES];
    }

    public MoveList GetOrderedMoves(ref MoveList moves, Move lastIteration, bool inQSearch, int ply)
    {
        enemyPawnAttackMap = board.MoveGenerator.EnemyPawnAttackMap();
        enemyAttackMapNoPawn = board.MoveGenerator.EnemyAttackMapNoPawn();
        enemyAttackMap = board.MoveGenerator.EnemyAttackMap();

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
            int color = board.State.SideToMove ? 0 : 1;
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
        }
        
        return score;
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
