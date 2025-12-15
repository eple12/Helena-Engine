namespace H.Core;

public static class SquareHelper
{
    // Assume that 0 <= rank <= 7 and 0 <= file <= 7 for performance
    public static Square GetSquare(int file, int rank)
    {
        return (Square) (rank * 8 + file); // 0~63
    }
    public static int GetFile(Square square)
    {
        return square % 8;
    }
    public static int GetRank(Square square)
    {
        return square / 8;
    }

    public static string ToString(Square square)
    {
        if (square == INVALID_SQUARE)
        {
            return "NN";
        }
        return $"{(char)('a' + GetFile(square))}{(char)('1' + GetRank(square))}";
    }
    public static string ToString(int file, int rank)
    {
        if (rank == 8) // Assume that the only possible invalid square is INVALID_SQUARE
        {
            return "NN";
        }
        return $"{'a' + file}{'1' + rank}";
    }

    public const Square INVALID_SQUARE = 64;

// region Square representations: A1~H8
    public const Square A1 = 0;
    public const Square B1 = 1;
    public const Square C1 = 2;
    public const Square D1 = 3;
    public const Square E1 = 4;
    public const Square F1 = 5;
    public const Square G1 = 6;
    public const Square H1 = 7;

    public const Square A2 = 8;
    public const Square B2 = 9;
    public const Square C2 = 10;
    public const Square D2 = 11;
    public const Square E2 = 12;
    public const Square F2 = 13;
    public const Square G2 = 14;
    public const Square H2 = 15;

    public const Square A3 = 16;
    public const Square B3 = 17;
    public const Square C3 = 18;
    public const Square D3 = 19;
    public const Square E3 = 20;
    public const Square F3 = 21;
    public const Square G3 = 22;
    public const Square H3 = 23;

    public const Square A4 = 24;
    public const Square B4 = 25;
    public const Square C4 = 26;
    public const Square D4 = 27;
    public const Square E4 = 28;
    public const Square F4 = 29;
    public const Square G4 = 30;
    public const Square H4 = 31;

    public const Square A5 = 32;
    public const Square B5 = 33;
    public const Square C5 = 34;
    public const Square D5 = 35;
    public const Square E5 = 36;
    public const Square F5 = 37;
    public const Square G5 = 38;
    public const Square H5 = 39;

    public const Square A6 = 40;
    public const Square B6 = 41;
    public const Square C6 = 42;
    public const Square D6 = 43;
    public const Square E6 = 44;
    public const Square F6 = 45;
    public const Square G6 = 46;
    public const Square H6 = 47;

    public const Square A7 = 48;
    public const Square B7 = 49;
    public const Square C7 = 50;
    public const Square D7 = 51;
    public const Square E7 = 52;
    public const Square F7 = 53;
    public const Square G7 = 54;
    public const Square H7 = 55;

    public const Square A8 = 56;
    public const Square B8 = 57;
    public const Square C8 = 58;
    public const Square D8 = 59;
    public const Square E8 = 60;
    public const Square F8 = 61;
    public const Square G8 = 62;
    public const Square H8 = 63;
// endregion

}