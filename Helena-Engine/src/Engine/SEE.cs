namespace H.Engine;

using H.Core;

// Important: IMPLEMENT PIN DATA UPDATE FIRST AND USE THAT CACHE DATA

public class SEE
{
    // [Type]
    // We only determine if a capture is "good" or "bad". We DO NOT evaluate them. So we just use constants, not tapered/tuned evaluation
    static readonly int[] MaterialValues = { 0, 100, 300, 300, 500, 900 };

    Board board;

    public SEE(Board _board)
    {
        board = _board;
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

        int promotionValue = MaterialValues[MoveFlag.GetPromType(move.Flag)];
        Piece targetPiece = board.At(move.Target);

        return promotionValue - (promotionValue != 0 ? MaterialValues[PieceHelper.PAWN] : 0)
         + (targetPiece != PieceHelper.NONE ? MaterialValues[PieceHelper.GetPieceType(targetPiece)] : 0);
    }

    // Type
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

                return i + 1;
            }
        }

        // Failsafe
        return PieceHelper.NONE;
    }

    Bitboard GetAllAttackersTo(Square square, Bitboard occupancy, Bitboard rooks, Bitboard bishops)
    {
        return (rooks & Magic.GetRookAttacks(square, occupancy)) | 
            (bishops & Magic.GetBishopAttacks(square, occupancy)) | 

            (board.BitboardSets[0][PieceHelper.PAWN] & Bits.PawnAttacks[1][square]) |  // Reverse White
            (board.BitboardSets[1][PieceHelper.PAWN] & Bits.PawnAttacks[0][square]) |  // Reverse Black

            (
                (board.BitboardSets[0][PieceHelper.KNIGHT] | board.BitboardSets[1][PieceHelper.KNIGHT]) & 
                Bits.KnightMovement[square]
            ) | 
            ((board.BitboardSets[0][PieceHelper.KING] | board.BitboardSets[1][PieceHelper.KING])
             & 
            Bits.KingMovement[square] & occupancy);
    }
}

