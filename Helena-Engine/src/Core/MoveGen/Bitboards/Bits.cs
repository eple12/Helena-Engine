namespace H.Core;

    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
    */
    // Copy this for cool visual bitboard comments

public static class Bits
{
    public const Bitboard FileA = 0x01010101_01010101;
    public const Bitboard FileH = FileA << 7;
    public const Bitboard NotFileA = ~FileA;
    public const Bitboard NotFileH = ~FileH;
    public const Bitboard Rank1 = 0b11111111ul;
    public const Bitboard Rank8 = 0b11111111ul << (8 * 7);

    public static readonly Bitboard[] KCastlingWay = [ (1ul << SquareHelper.F1) | (1ul << SquareHelper.G1), (1ul << SquareHelper.F8) | (1ul << SquareHelper.G8) ];
    public static readonly Bitboard[] QCastlingWay = [ (1ul << SquareHelper.C1) | (1ul << SquareHelper.D1), (1ul << SquareHelper.C8) | (1ul << SquareHelper.D8) ];
    public static readonly Bitboard[] QCastling3 = [ (1ul << SquareHelper.C1) | (1ul << SquareHelper.D1) | (1ul << SquareHelper.B1), (1ul << SquareHelper.C8) | (1ul << SquareHelper.D8) | (1ul << SquareHelper.B8) ];
    
    public static readonly Bitboard[] Files;
    public static readonly Bitboard[] Ranks;

// region Comment
    /* FILE C
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
        .  A  T  A  .  .  .  .
    */
    /* FILE A
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
        T  A  .  .  .  .  .  .
    */
    // A for Adjacent, T for Triple. T also includes A
// endregion
    public static readonly Bitboard[] AdjacentFiles;
    public static readonly Bitboard[] TripleFiles;

// region Directions
    public static readonly Coord[] RookDirections = { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1) };
    public static readonly Coord[] BishopDirections = { new Coord(1, 1), new Coord(-1, 1), new Coord(-1, -1), new Coord(1, -1) };
    public static readonly Coord[] KingRing = { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1), new Coord(1, 1), new Coord(-1, 1), new Coord(-1, -1), new Coord(1, -1) };
    public static readonly Coord[] KnightJump = { new Coord(2, 1), new Coord(1, 2), new Coord(-1, 2), new Coord(-2, 1), new Coord(-2, -1), new Coord(-1, -2), new Coord(1, -2), new Coord(2, -1) };

    public static readonly Coord[] DirectionCoords = [.. RookDirections, .. BishopDirections];
    public static readonly int[] DirectionOffsets = { 1, 8, -1, -8, 9, 7, -9, -7 };

    public static readonly int[][] NumSquaresToEdge; // [Square] [Direction] Direction: 0-7
    // Bits towards the direction from Square (Including the square)
    public static readonly Bitboard[][] DirRayMasks; // [Square] [Direction]
    // Draw a line with two squares
    public static readonly Bitboard[][] AlignMasks; // [Square1] [Square2]
// endregion


// region Move Generation
    public static readonly Bitboard[] KingMovement;
    public static readonly Bitboard[] KnightMovement;
    public static readonly Bitboard[][] PawnAttacks; // [Color] [Square]
// endregion

    static Bits()
    {
        Files = new Bitboard[8];
        for (int file = 0; file < 8; file++)
        {
            Files[file] = FileA << file;
        }
        Ranks = new Bitboard[8];
        for (int rank = 0; rank < 8; rank++)
        {
            Ranks[rank] = Rank1 << (rank * 8);
        }

        AdjacentFiles = new Bitboard[8];
        TripleFiles = new Bitboard[8];
        for (int file = 0; file < 8; file++)
        {
            Bitboard left = file > 0 ? Files[file - 1] : 0;
            Bitboard right = file < 7 ? Files[file + 1] : 0;

            AdjacentFiles[file] = left | right;
            TripleFiles[file] = AdjacentFiles[file] | Files[file];
        }

        KingMovement = new Bitboard[64];
        KnightMovement = new Bitboard[64];

        // King / Knight
        for (Square i = 0; i < 64; i++)
        {
            Coord c = new Coord(i);

            // King
            foreach (Coord offset in KingRing)
            {
                Coord target = c + offset;
                if (target.IsValid)
                {
                    KingMovement[i] |= 1ul << target.GetSquare;
                }
            }

            // Knight
            foreach (Coord offset in KnightJump)
            {
                Coord target = c + offset;
                if (target.IsValid)
                {
                    KnightMovement[i] |= 1ul << target.GetSquare;
                }
            }
        }

        NumSquaresToEdge = new int[64][];
        DirRayMasks = new Bitboard[64][];
        for (Square i = 0; i < 64; i++)
        {
            NumSquaresToEdge[i] = new int[8];
            DirRayMasks[i] = new Bitboard[8];
            for (int dir = 0; dir < 8; dir++)
            {
                Coord c = new Coord(i);

                DirRayMasks[i][dir].SetSquare(i);

                int count = 0;
                for (int dst = 1; dst <= 7; dst++)
                {
                    Coord target = c + DirectionCoords[dir] * dst;
                    if (target.IsValid)
                    {
                        count++;

                        Square s = target.GetSquare;
                        DirRayMasks[i][dir].SetSquare(s);
                    }
                    else
                    {
                        break;
                    }
                }

                NumSquaresToEdge[i][dir] = count;
            }
        }

        PawnAttacks = new Bitboard[2][];
        for (int color = 0; color < 2; color++)
        {
            PawnAttacks[color] = new Bitboard[64];

            for (Square sq = 0; sq < 64; sq++)
            {
                PawnAttacks[color][sq] = BitboardHelper.PawnAttacks(1ul << sq, (Color) color);
            }
        }

        AlignMasks = new Bitboard[64][];
        for (Square squareA = 0; squareA < 64; squareA++)
        {
            AlignMasks[squareA] = new Bitboard[64];
            for (Square squareB = 0; squareB < 64; squareB++)
            {
                Coord cA = new Coord(squareA);
                Coord cB = new Coord(squareB);
                Coord delta = cB - cA;
                Coord dir = new Coord(System.Math.Sign(delta.X), System.Math.Sign(delta.Y));

                for (int i = -8; i < 8; i++)
                {
                    Coord coord = cA + dir * i;
                    if (coord.IsValid)
                    {
                        AlignMasks[squareA][squareB] |= 1ul << coord.GetSquare;
                    }
                }
            }
        }
    }
}