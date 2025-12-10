namespace H.Core;

public static class PieceHelper
{
    // Piece uses 3 least significant bits for type and 1 bit for color
    // Assume that only 4 least significant bits are used, and the rest are 0 for performance
    public const PieceType NONE = 0;
    public const PieceType PAWN = 1;
    public const PieceType KNIGHT = 2;
    public const PieceType BISHOP = 3;
    public const PieceType ROOK = 4;
    public const PieceType QUEEN = 5;
    public const PieceType KING = 6;

    public const Color WHITE = 0;
    public const Color BLACK = 1;

    public const byte ColorMask = 0b0000_1000;
    public const byte TypeMask = 0b0000_0111;

    public static Piece Make(PieceType type, Color color)
    {
        return (Piece) (color << 3 | type);
    }
    public static bool IsColor(Piece piece, Color color)
    {
        return (piece & ColorMask) == color << 3;
    }
    public static Color GetColor(Piece piece)
    {
        return (Color) ((piece & ColorMask) >> 3);
    }
    public static PieceType GetPieceType(Piece piece)
    {
        return (PieceType) (piece & TypeMask);
    }

    // Converts a boolean into Color; this is for simplicity
    public static Color GetColor(bool isWhite)
    {
        return isWhite ? WHITE : BLACK;
    }

    // For displaying the piece
    public static char ToChar(Piece piece)
    {
        PieceType type = GetPieceType(piece);

        char c = type switch
        {
            PAWN => 'p',
            KNIGHT => 'n',
            BISHOP => 'b',
            ROOK => 'r',
            QUEEN => 'q',
            KING => 'k',
            _ => '.'
        };

        if (IsColor(piece, WHITE))
        {
            c = char.ToUpper(c);
        }

        return c;
    }
}