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
        int score = MaterialValues[PieceHelper.GetPieceType(board.At(move.Target))] - threshold; // Gain() - threshold

        if (score < 0)
        {
            return false;
        }

        Piece next = PieceHelper.GetPieceType(board.At(move.Start));
        score -= MaterialValues[next];

        if (score >= 0)
        {
            return true;
        }

        Bitboard whiteOccupancy = board.BitboardSets[0].All;
        Bitboard blackOccupancy = board.BitboardSets[1].All;
        Bitboard occupancy = whiteOccupancy | blackOccupancy;
        occupancy.ToggleSquare(move.Start, move.Target);

        // All sliders
        Bitboard queens = board.BitboardSets[0][PieceHelper.QUEEN] | board.BitboardSets[1][PieceHelper.QUEEN];
        Bitboard rooks = board.BitboardSets[0][PieceHelper.ROOK] | board.BitboardSets[1][PieceHelper.ROOK] | queens;
        Bitboard bishops = board.BitboardSets[0][PieceHelper.BISHOP] | board.BitboardSets[1][PieceHelper.BISHOP] | queens;

        Bitboard attackers = board.GetAllAttackersTo(move.Target, occupancy, rooks, bishops);

        // We made a capture, now it is opponent's turn
        bool us = !board.State.SideToMove;

        while (true)
        {
            Bitboard ourAttackers = attackers & (us ? whiteOccupancy : blackOccupancy);

            if (ourAttackers == 0)
            {
                break;
            }

            int nextPieceType = PopLeastValuableAttacker(ref occupancy, ourAttackers, PieceHelper.GetColor(us));

            if (nextPieceType == PieceHelper.PAWN || PieceHelper.IsDiagonal((Piece) nextPieceType))
            {
                attackers |= Magic.GetBishopAttacks(move.Target, occupancy) & bishops;
            }
            else if (PieceHelper.IsOrthogonal((Piece) nextPieceType))
            {
                attackers |= Magic.GetRookAttacks(move.Target, occupancy) & rooks;
            }

            attackers &= occupancy;

            score = -score - 1 -MaterialValues[nextPieceType];

            us = !us;

            if (score >= 0)
            {
                if ((nextPieceType == PieceHelper.KING) && (attackers & (us ? whiteOccupancy : blackOccupancy)) != 0)
                {
                    us = !us;
                }

                break;
            }
        }

        return us != board.State.SideToMove;
    }

    public bool HasPositiveScore(Move move, int threshold = 0)
    {
        int score = Gain(move) - threshold;

        if (score < 0)
        {
            return false;
        }

        Piece next = MoveFlag.IsPromotion(move.Flag) ? MoveFlag.GetPromType(move.Flag) : PieceHelper.GetPieceType(board.At(move.Start));
        score -= MaterialValues[next];
        // If risking the capturing piece still has a positive score, then this move must be good anyway
        if (score >= 0)
        {
            return true;
        }

        Bitboard whiteOccupancy = board.BitboardSets[0].All;
        Bitboard blackOccupancy = board.BitboardSets[1].All;
        Bitboard occupancy = whiteOccupancy | blackOccupancy;
        occupancy.ToggleSquare(move.Start, move.Target);

        // All sliders
        Bitboard queens = board.BitboardSets[0][PieceHelper.QUEEN] | board.BitboardSets[1][PieceHelper.QUEEN];
        Bitboard rooks = board.BitboardSets[0][PieceHelper.ROOK] | board.BitboardSets[1][PieceHelper.ROOK] | queens;
        Bitboard bishops = board.BitboardSets[0][PieceHelper.BISHOP] | board.BitboardSets[1][PieceHelper.BISHOP] | queens;

        Bitboard attackers = board.GetAllAttackersTo(move.Target, occupancy, rooks, bishops);

        // We made a capture, now it is opponent's turn
        bool us = !board.State.SideToMove;

        while (true)
        {
            Bitboard ourAttackers = attackers & (us ? whiteOccupancy : blackOccupancy);

            if (ourAttackers == 0)
            {
                break;
            }

            int nextPieceType = PopLeastValuableAttacker(ref occupancy, ourAttackers, PieceHelper.GetColor(us));

            if (nextPieceType == PieceHelper.PAWN || PieceHelper.IsDiagonal((Piece) nextPieceType))
            {
                attackers |= Magic.GetBishopAttacks(move.Target, occupancy) & bishops;
            }
            else if (PieceHelper.IsOrthogonal((Piece) nextPieceType))
            {
                attackers |= Magic.GetRookAttacks(move.Target, occupancy) & rooks;
            }

            attackers &= occupancy;

            score = -score - 1 -MaterialValues[nextPieceType];

            us = !us;

            if (score >= 0)
            {
                if ((nextPieceType == PieceHelper.KING) && (attackers & (us ? whiteOccupancy : blackOccupancy)) != 0)
                {
                    us = !us;
                }

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

