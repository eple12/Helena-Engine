namespace H.Core;

public struct Move
{
    // We use 16 bits for a move.
    // .... .... .... ....
    // [  ] [     ][     ]
    // flag / start / target
    ushort moveValue;

    const ushort FlagMask = 0b1111_0000_0000_0000;
    const ushort StartMask = 0b0000_1111_1100_0000;
    const ushort TargetMask = 0b0000_0000_0011_1111;

    public ushort Flag => (ushort) ((moveValue & FlagMask) >> 12);
    public Square Start => (Square) ((moveValue & StartMask) >> 6);
    public Square Target => (Square) (moveValue & TargetMask);
    public ushort MoveValue => moveValue;

    public static Move NullMove = new(0);


    public string Notation {
        get{
            if (!MoveFlag.IsPromotion(Flag))
            {
                return $"{SquareHelper.ToString(Start)}{SquareHelper.ToString(Target)}";
            }
            PieceType type = MoveFlag.GetPromType(Flag);
            return $"{SquareHelper.ToString(Start)}{SquareHelper.ToString(Target)}{char.ToLower(PieceHelper.ToChar(type))}";
        }
    }

    public Move(ushort value)
    {
        moveValue = value;
    }
    public Move(Square start, Square target)
    {
        moveValue = (ushort) ((start << 6) | target);
    }
    public Move(Square start, Square target, ushort flag)
    {
        moveValue = (ushort) ((flag << 12) | (start << 6) | target);
    }

    public static bool operator==(Move a, Move b)
    {
        return a.moveValue == b.moveValue;
    }
    public static bool operator!=(Move a, Move b)
    {
        return !(a == b);
    }
}

public struct MoveFlag
{
    // 16 standard move flags
    public const ushort None = 0;
    public const ushort PawnTwo = 1; // Pushing a pawn 2 squares forward
    public const ushort KCastling = 2;
    public const ushort QCastling = 3;
    public const ushort Capture = 4;
    public const ushort EP = 5; // En passant
    // For some reason 6-7 is missing
    public const ushort NProm = 8;
    public const ushort BProm = 9;
    public const ushort RProm = 10;
    public const ushort QProm = 11;
    public const ushort NPromCapture = 12;
    public const ushort BPromCapture = 13;
    public const ushort RPromCapture = 14;
    public const ushort QPromCapture = 15;

    public static bool IsCapture(ushort flag)
    {
        return (flag == Capture) || (flag > 11);
    }
    public static bool IsCastling(ushort flag)
    {
        return (flag == KCastling) || (flag == QCastling);
    }
    public static bool IsPromotion(ushort flag)
    {
        return flag >= 8;
    }

    // Get Promotion PieceType
    public static PieceType GetPromType(ushort flag)
    {
        if (flag < 8)
        {
            return PieceHelper.NONE;
        }

        if (flag < 12)
        {
            return (byte) (PieceHelper.KNIGHT + flag - 8);
        }

        return (byte) (PieceHelper.KNIGHT + flag - 12);
    }

    // Assume that the type is correct for performance
    // This is used in Move Generation
    public static ushort GetPromFlag(PieceType type, bool capture)
    {
        return (ushort) (type + 6 + (capture ? 4 : 0));
    }
}