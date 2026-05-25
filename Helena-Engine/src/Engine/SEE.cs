namespace H.Engine;

using H.Core;

// Note: DOES NOT consider pinned pieces, since it is rare to have pinned pieces in capturing sequence.
// SEE is not a precise calculation, it is just an estimation.
// Does not calculate pins for performance... but still calculates X-Ray attacks though.
public class SEE
{
    // [Type]
    // We only determine if a capture is "good" or "bad". We DO NOT evaluate them. So we just use constants, not tapered/tuned evaluation
    static readonly int[] MaterialValues = EvaluationConstants.AbsoluteMaterial;

    Board board;

    public SEE(Board _board)
    {
        board = _board;
    }

    // DOES NOT HANDLE PROMOTIONS AND QUIET MOVES
    // ONLY PURE CAPTURES
    public bool IsGoodCapture(Move move, int threshold = 0)
    {
        // Initial gain: value of the piece on the target square
        int score = MaterialValues[PieceHelper.GetPieceType(board.At(move.Target))] - threshold;
        if (score < 0) return false;

        score -= MaterialValues[PieceHelper.GetPieceType(board.At(move.Start))];
        if (score >= 0) return true;

        return RunSEELoop(move, score);
    }

    public bool HasPositiveScore(Move move, int threshold = 0)
    {
        int score = Gain(move) - threshold;
        if (score < 0) return false;

        // For promotions, the moving piece value is the promoted piece type, not a pawn
        Piece next = MoveFlag.IsPromotion(move.Flag) ? MoveFlag.GetPromType(move.Flag) : PieceHelper.GetPieceType(board.At(move.Start));
        score -= MaterialValues[next];
        // If risking the capturing piece still has a positive score, then this move must be good anyway
        if (score >= 0) return true;

        return RunSEELoop(move, score);
    }

    // Shared SEE exchange loop. Called after the first capture has already been applied to score.
    // Simulates all subsequent recaptures on move.Target and returns true if the side to move gains.
    bool RunSEELoop(Move move, int score)
    {
        Bitboard whiteOccupancy = board.BitboardSets[0].All;
        Bitboard blackOccupancy = board.BitboardSets[1].All;
        Bitboard occupancy = whiteOccupancy | blackOccupancy;
        occupancy.ToggleSquare(move.Start, move.Target);

        Bitboard queens  = board.BitboardSets[0][PieceHelper.QUEEN]  | board.BitboardSets[1][PieceHelper.QUEEN];
        Bitboard rooks   = board.BitboardSets[0][PieceHelper.ROOK]   | board.BitboardSets[1][PieceHelper.ROOK]   | queens;
        Bitboard bishops = board.BitboardSets[0][PieceHelper.BISHOP] | board.BitboardSets[1][PieceHelper.BISHOP] | queens;

        Bitboard attackers = board.GetAllAttackersTo(move.Target, occupancy, rooks, bishops);

        // First capture was by the side to move, so the next recapturer is the opponent
        bool us = !board.State.SideToMove;

        while (true)
        {
            Bitboard ourAttackers = attackers & (us ? whiteOccupancy : blackOccupancy);
            if (ourAttackers == 0) break;

            int nextPieceType = PopLeastValuableAttacker(ref occupancy, ourAttackers, PieceHelper.GetColor(us));

            // Reveal any X-ray attackers behind the piece that just moved
            if (nextPieceType == PieceHelper.PAWN || PieceHelper.IsDiagonal((Piece) nextPieceType))
                attackers |= Magic.GetBishopAttacks(move.Target, occupancy) & bishops;
            else if (PieceHelper.IsOrthogonal((Piece) nextPieceType))
                attackers |= Magic.GetRookAttacks(move.Target, occupancy) & rooks;

            attackers &= occupancy;

            score = -score - 1 - MaterialValues[nextPieceType];
            us = !us;

            if (score >= 0)
            {
                // King cannot recapture if the opponent still has attackers
                if (nextPieceType == PieceHelper.KING && (attackers & (us ? whiteOccupancy : blackOccupancy)) != 0)
                    us = !us;
                break;
            }
        }

        return us != board.State.SideToMove;
    }

    int Gain(Move move)
    {
        if (move.Flag < MoveFlag.Capture) // Quiet / Castling / PawnTwo
        {
            return 0;
        }
        else if (move.Flag == MoveFlag.EP)
        {
            return MaterialValues[PieceHelper.PAWN]; // Pawn
        }

        int promotionValue = MaterialValues[MoveFlag.GetPromType(move.Flag)]; // [Piece Type]
        Piece targetPieceType = PieceHelper.GetPieceType(board.At(move.Target));

        return promotionValue == 0 ? MaterialValues[targetPieceType] : promotionValue - MaterialValues[PieceHelper.PAWN] + MaterialValues[targetPieceType];
    }

    // Returns Type
    // Updates the occupancy
    int PopLeastValuableAttacker(ref Bitboard occupancy, Bitboard attackers, Color color)
    {
        // Loop: Pawn to King
        for (int i = 0; i < 6; i++)
        {
            Bitboard overlap = attackers & board.BitboardSets[color].Indexed(i);

            if (overlap != 0)
            {
                int square = overlap.PopLSB();
                occupancy ^= 1ul << square;

                return i + 1; // Piece Type
            }
        }

        // Failsafe
        return PieceHelper.NONE;
    }
}

