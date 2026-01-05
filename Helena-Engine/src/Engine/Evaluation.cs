namespace H.Engine;

using H.Core;
using H.Program;

// using static EvaluationConstants;
using static TunedEvaluation;
using static EvaluationHelper;

public static class Evaluation
{
    const int MATE_EVAL = 100000;

    static Board board = Main.MainBoard;

    // Evaluation data
    static int[][] pieceCount;
    static int phase;
    static readonly int[] PhaseValues = { 0, 1, 1, 2, 4, 0 };
    const int MAX_TOTAL_PHASE_VALUE = 24; 

    static bool isWhiteToMove;
    static int stmSign;
    static Color friendlyColor;
    static Color enemyColor;
    static int materialDelta;

    static Evaluation()
    {
        // Logic from EvalData constructor
        pieceCount = new int[2][];
        pieceCount[0] = new int[6];
        pieceCount[1] = new int[6];

        // Always 1 king (pieceType 5) - not directly part of phase sum but good to initialize
        pieceCount[0][5] = 1;
        pieceCount[1][5] = 1;

        phase = 0; // Initialize phase
    }

    static void Update()
    {
        int currentPhaseSum = 0;
        for (int color = 0; color < 2; color++)
        {
            for (int pieceType = 0; pieceType < 5; pieceType++) // Iterate Pawn (0) to Queen (4)
            {
                pieceCount[color][pieceType] = board.BitboardSets[color].Indexed(pieceType).Count();
                currentPhaseSum += pieceCount[color][pieceType] * PhaseValues[pieceType];
            }
            // King (PieceType 5) is not included in phase calculation based on common practice.
        }

        // Clamp the current phase sum to ensure it doesn't exceed the max
        // (e.g., if a non-standard setup has more pieces than initial)
        currentPhaseSum = Math.Min(currentPhaseSum, MAX_TOTAL_PHASE_VALUE);
        // currentPhaseSum = Math.Max(currentPhaseSum, 0); // Ensure non-negative

        // Normalize to 0-256 range
        phase = currentPhaseSum * 256 / MAX_TOTAL_PHASE_VALUE;
        
        isWhiteToMove = board.State.SideToMove;
        stmSign = isWhiteToMove ? 1 : -1;
        friendlyColor = isWhiteToMove ? PieceHelper.WHITE : PieceHelper.BLACK;
        enemyColor = isWhiteToMove ? PieceHelper.BLACK : PieceHelper.WHITE;
    }

    public static int Eval(bool verbose = false)
    {
        Update();

        TaperedScore eval = new (0, 0);

        // I don't know if we really need piece mobility eval

        eval += Material();
        eval += MopUp();

        eval += PSQTValue();
        
        eval += Outpost(friendlyColor);
        eval -= Outpost(enemyColor);

        eval += OpenFileValue(friendlyColor);
        eval -= OpenFileValue(enemyColor);

        eval += PawnValue(friendlyColor);
        eval -= PawnValue(enemyColor);

        TaperedScore kingEval = new(0, 0);
        kingEval += KingSafety(friendlyColor);
        kingEval -= KingSafety(enemyColor);

        if (verbose)
        {
            System.Console.WriteLine(kingEval[phase]);
        }
        
        eval += kingEval;

        return eval[phase];
    }

    static TaperedScore Material()
    {
        TaperedScore val = new (0, 0);
        for (int i = 0; i < 5; i++)
        {
            val += MaterialValues[i] * (pieceCount[friendlyColor][i] - pieceCount[enemyColor][i]);
        }
        materialDelta = val[phase];
        return val;
    }
    static readonly TaperedScore[][] PsqtTables = {
        // PSQT.Pawns,
        // PSQT.Knights,
        // PSQT.Bishops,
        // PSQT.Rooks,
        // PSQT.Queens,
        // PSQT.King
        PawnPsqt,
        KnightPsqt,
        BishopPsqt,
        RookPsqt,
        QueenPsqt,
        KingPsqt
    };
    static TaperedScore PSQTValue()
    {
        TaperedScore total = new TaperedScore(0, 0);

        for (int color = 0; color < 2; color++)
        {
            for (int pieceType = 0; pieceType < 6; pieceType++)
            {
                ulong bitboard = board.BitboardSets[color].Indexed(pieceType);
                TaperedScore[] table = PsqtTables[pieceType];

                while(bitboard != 0)
                {
                    int sq = bitboard.PopLSB();
                    if (color == 0) // White
                    {
                        // Flip white because we wrote the table upside down
                        total += table[SquareHelper.FlipRank((Square)sq)];
                    }
                    else // Black
                    {
                        total -= table[sq];
                    }
                }
            }
        }

        return total * stmSign;
    }
    static TaperedScore MopUp()
    {
        TaperedScore eval = new (0, 0);

        // Assume that we are winning in an endgame so use endgame pawn value
        if (materialDelta >= MaterialValues[0].End)
        {
            Square friendlyKingSquare = board.KingSquares[friendlyColor];
            Square enemyKingSquare = board.KingSquares[enemyColor];

            eval += (14 - DistanceFromSquare[friendlyKingSquare, enemyKingSquare]) * CloserToEnemyKing;
            eval += DistanceFromCenter[enemyKingSquare] * EnemyKingCorner;

            // Perhaps add logic for bishop & knight mates
        }

        return eval;
    }
    static TaperedScore Outpost(Color color) // For Knights and Bishops
    {
        TaperedScore eval = new (0, 0);

        Bitboard knights = board.BitboardSets[color][PieceHelper.KNIGHT];
        Bitboard bishops = board.BitboardSets[color][PieceHelper.BISHOP];

        while (knights != 0)
        {
            Square sq = (Square) knights.PopLSB();

            // No enemy pawn can kick this knight out and the knight is supported by our pawns
            if (((ForwardPawnAttackers[color][sq] & board.BitboardSets[1 - color][PieceHelper.PAWN]) == 0) && IsSupportedByMyPawn(color, sq))
            {
                eval += OutpostBonus;
            }
        }

        while (bishops != 0)
        {
            Square sq = (Square) bishops.PopLSB();

            // No enemy pawn can kick this knight out and the knight is supported by our pawns
            if (((ForwardPawnAttackers[color][sq] & board.BitboardSets[1 - color][PieceHelper.PAWN]) == 0) && IsSupportedByMyPawn(color, sq))
            {
                eval += OutpostBonus;
            }
        }

        return eval;
    }
    static TaperedScore OpenFileValue(Color color)
    {
        TaperedScore eval = new (0, 0);
        Bitboard rooks = board.BitboardSets[color][PieceHelper.ROOK];
        while (rooks != 0)
        {
            Square sq = (Square) rooks.PopLSB();
            int file = SquareHelper.GetFile(sq);

            if (IsOpenFile(file))
            {
                eval += OpenFileBonus;
            }
            else if (IsSemiOpenFile(file, color))
            {
                eval += SemiFileBonus;
            }
        }

        return eval;
    }
    static TaperedScore PawnValue(Color color)
    {
        Bitboard friendlyPawns = board.BitboardSets[color][PieceHelper.PAWN];
        Bitboard enemyPawns = board.BitboardSets[1 - color][PieceHelper.PAWN];

        TaperedScore eval = new (0, 0);

        Bitboard friendlyPawnsCopy = friendlyPawns;
        int numIsolated = 0;
        while (friendlyPawnsCopy != 0)
        {
            Square sq = (Square) friendlyPawnsCopy.PopLSB();
            int rank = SquareHelper.GetRank(sq);
            int file = SquareHelper.GetFile(sq);

            Bitboard passedMask = PassedPawnMask[color][sq];
            if ((passedMask & enemyPawns) == 0)
            {
                eval += PassedPawnBonus[color == 0 ? rank : 7 - rank];
            }

            if ((friendlyPawns & Bits.AdjacentFiles[file]) == 0)
            {
                numIsolated++;
            }
        }

        eval -= numIsolated * IsolatedPawnPenaltyPerPawn;

        return eval;
    }
    static TaperedScore KingSafety(Color color)
    {
        TaperedScore eval = new (0, 0);
        Square kingSquare = board.KingSquares[color];
        int kingFile = SquareHelper.GetFile(kingSquare);

        Color opponent = (Color)(1 - color);
        Bitboard friendlyPawns = board.BitboardSets[color][PieceHelper.PAWN];

        // 1. Pawn Shelter & File Openings
        int shelterBaseRank = (color == PieceHelper.WHITE) ? 1 : 6;
        for (int file = Math.Max(0, kingFile - 1); file <= Math.Min(7, kingFile + 1); file++)
        {
            Bitboard fileBB = Bits.Files[file];
            Bitboard shelterPawns = friendlyPawns & fileBB;

            if (shelterPawns == 0) // No friendly pawn on this file
            {
                eval -= PawnShelterMissingPenalty;
                // Check for open files
                if ((board.BitboardSets[opponent][PieceHelper.PAWN] & fileBB) == 0) // Fully open
                    eval -= KingFileOpenPenalty;
                else // Semi-open for opponent
                    eval -= KingFileSemiOpenPenalty;
            }
            else
            {
                Square pawnSquare = (Square) ((color == PieceHelper.WHITE) ? shelterPawns.PopLSB() : shelterPawns.PopMSB());
                int pawnRank = SquareHelper.GetRank(pawnSquare);
                int rankDist = Math.Abs(pawnRank - shelterBaseRank);
                if (rankDist > 1) // Pawns advanced more than one step from base are weak
                {
                    eval -= PawnShelterWeakPenalty * (rankDist -1);
                }
            }
        }

        // // 3. King Attacks
        // int attackUnits = 0;
        // Bitboard kingZone = KingArea[kingSquare];
        // Bitboard allPieces = board.BitboardSets[0].All | board.BitboardSets[1].All;

        // // Knights
        // Bitboard attackers = board.BitboardSets[opponent][PieceHelper.KNIGHT];
        // while (attackers != 0)
        // {
        //     Square attackerSq = (Square) attackers.PopLSB();
        //     if ((Bits.KnightMovement[attackerSq] & kingZone) != 0)
        //         attackUnits += AttackerWeight[PieceHelper.KNIGHT];
        // }
        // // Bishops
        // attackers = board.BitboardSets[opponent][PieceHelper.BISHOP];
        // while (attackers != 0)
        // {
        //     Square attackerSq = (Square) attackers.PopLSB();
        //     if ((Magic.GetBishopAttacks(attackerSq, allPieces) & kingZone) != 0)
        //         attackUnits += AttackerWeight[PieceHelper.BISHOP];
        // }
        // // Rooks
        // attackers = board.BitboardSets[opponent][PieceHelper.ROOK];
        // while (attackers != 0)
        // {
        //     Square attackerSq = (Square) attackers.PopLSB();
        //     if ((Magic.GetRookAttacks(attackerSq, allPieces) & kingZone) != 0)
        //         attackUnits += AttackerWeight[PieceHelper.ROOK];
        // }
        // // Queens
        // attackers = board.BitboardSets[opponent][PieceHelper.QUEEN];
        // while (attackers != 0)
        // {
        //     Square attackerSq = (Square) attackers.PopLSB();
        //     if (((Magic.GetBishopAttacks(attackerSq, allPieces) | Magic.GetRookAttacks(attackerSq, allPieces)) & kingZone) != 0)
        //         attackUnits += AttackerWeight[PieceHelper.QUEEN];
        // }


        // attackUnits = Math.Min(attackUnits, KingSafetyPenaltyTableLength - 1);
        // eval -= KingSafetyPenaltyTable[attackUnits];

        return eval;
    }

    static bool IsSemiOpenFile(int file, Color color) // No friendly pawn on the file
    {
        return (Bits.Files[file] & board.BitboardSets[color][PieceHelper.PAWN]) == 0;
    }
    static bool IsOpenFile(int file)
    {
        return (Bits.Files[file] & (board.BitboardSets[friendlyColor][PieceHelper.PAWN] | board.BitboardSets[enemyColor][PieceHelper.PAWN])) == 0;
    }
    static bool IsSupportedByMyPawn(Color color, Square square)
    {
        Bitboard enemyPawnAttack = Bits.PawnAttacks[1 - color][square];
        return (enemyPawnAttack & board.BitboardSets[color][PieceHelper.PAWN]) != 0;
    }

    // For move ordering
    // Use Mid values for performance
    public static int GetMaterialValue(PieceType type)
    {
        if (type == PieceHelper.NONE || type == PieceHelper.KING)
        {
            return 0;
        }
        return MaterialValues[type - 1].Mid;
    }
    public static int GetPsqtScore(PieceType type, int color, int sq)
    {
        if (type == PieceHelper.NONE)
        {
            return 0;
        }

        int pieceTypeIdx = type - 1;
        TaperedScore[] table = PsqtTables[pieceTypeIdx];

        if (color == 0) // White
        {
            return table[SquareHelper.FlipRank((Square)sq)].Mid;
        }
        else // Black
        {
            return -table[sq].Mid;
        }
    }


    public static int MateEval(int plyFromRoot)
    {
        return MATE_EVAL - plyFromRoot;
    }

    public static bool IsMateEval(int eval)
    {
        return Math.Abs(eval) >= MATE_EVAL - Constants.MAX_DEPTH;
    }
    public static int MateInPly(int eval)
    {
        return MATE_EVAL - Math.Abs(eval);
    }
}

public struct TaperedScore
{
    public int value;

    public TaperedScore(int m, int e)
    {
        value = e;
        value += m << 16;
    }
    public TaperedScore(int v)
    {
        value = v;
    }

    public int Mid => value >> 16;
    public int End => (short)(value & 0xFFFF);


    public int this[int phase]
    {
        get
        {
            return (Mid * phase + End * (256 - phase)) >> 8;
        }
    }

    public int GetValue(int phase)
    {
        // Mid = 256 End = 0
        return this[phase];
    }

    public static TaperedScore operator +(TaperedScore a, TaperedScore b)
    {
        return new TaperedScore(a.value + b.value);
    }
    
    public static TaperedScore operator -(TaperedScore a, TaperedScore b)
    {
        return new TaperedScore(a.value - b.value);
    }

    public static TaperedScore operator *(TaperedScore a, int m)
    {
        return new TaperedScore(a.value * m);
    }
    public static TaperedScore operator *(int m, TaperedScore a)
    {
        return new TaperedScore(a.value * m);
    }
    public static TaperedScore operator /(TaperedScore a, int m)
    {
        return new TaperedScore(a.value / m);
    }
}
