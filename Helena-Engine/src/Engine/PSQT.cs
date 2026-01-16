namespace H.Engine;

using H.Core;
using S = TaperedScore;

public static class PSQT
{
    // Pawns & PawnsEnd combined
    public static readonly S[] Pawns = {
        new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0),
        new S( 50, 80), new S( 50, 80), new S( 50, 80), new S( 50, 80), new S( 50, 80), new S( 50, 80), new S( 50, 80), new S( 50, 80),
        new S( 10, 50), new S( 10, 50), new S( 20, 50), new S( 30, 50), new S( 30, 50), new S( 20, 50), new S( 10, 50), new S( 10, 50),
        new S(  5, 30), new S(  5, 30), new S( 10, 30), new S( 25, 30), new S( 25, 30), new S( 10, 30), new S(  5, 30), new S(  5, 30),
        new S(  0, 20), new S(  0, 20), new S(  0, 20), new S( 20, 20), new S( 20, 20), new S(  0, 20), new S(  0, 20), new S(  0, 20),
        new S(  5, 10), new S( -5, 10), new S(-10, 10), new S(  0, 10), new S(  0, 10), new S(-10, 10), new S( -5, 10), new S(  5, 10),
        new S(  5, 10), new S( 10, 10), new S( 10, 10), new S(-20, 10), new S(-20, 10), new S( 10, 10), new S( 10, 10), new S(  5, 10),
        new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0),
    };
    
    // Knights - use same for mid and end
    public static readonly S[] Knights = {
        new S(-50,-50), new S(-40,-40), new S(-30,-30), new S(-30,-30), new S(-30,-30), new S(-30,-30), new S(-40,-40), new S(-50,-50),
        new S(-40,-40), new S(-20,-20), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(-20,-20), new S(-40,-40),
        new S(-30,-30), new S(  0,  0), new S( 10, 10), new S( 15, 15), new S( 15, 15), new S( 10, 10), new S(  0,  0), new S(-30,-30),
        new S(-30,-30), new S(  5,  5), new S( 15, 15), new S( 20, 20), new S( 20, 20), new S( 15, 15), new S(  5,  5), new S(-30,-30),
        new S(-30,-30), new S(  0,  0), new S( 15, 15), new S( 20, 20), new S( 20, 20), new S( 15, 15), new S(  0,  0), new S(-30,-30),
        new S(-30,-30), new S(  5,  5), new S( 10, 10), new S( 15, 15), new S( 15, 15), new S( 10, 10), new S(  5,  5), new S(-30,-30),
        new S(-40,-40), new S(-20,-20), new S(  0,  0), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S(-20,-20), new S(-40,-40),
        new S(-50,-50), new S(-40,-40), new S(-30,-30), new S(-30,-30), new S(-30,-30), new S(-30,-30), new S(-40,-40), new S(-50,-50)
    };

    // Bishops - use same for mid and end
    public static readonly S[] Bishops =  {
        new S(-20,-20), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-20,-20),
        new S(-10,-10), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(-10,-10),
        new S(-10,-10), new S(  0,  0), new S(  5,  5), new S( 10, 10), new S( 10, 10), new S(  5,  5), new S(  0,  0), new S(-10,-10),
        new S(-10,-10), new S(  5,  5), new S(  5,  5), new S( 10, 10), new S( 10, 10), new S(  5,  5), new S(  5,  5), new S(-10,-10),
        new S(-10,-10), new S(  0,  0), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S(  0,  0), new S(-10,-10),
        new S(-10,-10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S(-10,-10),
        new S(-10,-10), new S(  5,  5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  5,  5), new S(-10,-10),
        new S(-20,-20), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-10,-10), new S(-20,-20)
    };

    // Rooks - use same for mid and end
    public static readonly S[] Rooks =  {
        new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0),
        new S(  5,  5), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S( 10, 10), new S(  5,  5),
        new S( -5, -5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S( -5, -5),
        new S( -5, -5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S( -5, -5),
        new S( -5, -5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S( -5, -5),
        new S( -5, -5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S( -5, -5),
        new S( -5, -5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S( -5, -5),
        new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S(  0,  0), new S(  0,  0)
    };

    // Queens - use same for mid and end
    public static readonly S[] Queens =  {
        new S(-20,-20), new S(-10,-10), new S(-10,-10), new S( -5, -5), new S( -5, -5), new S(-10,-10), new S(-10,-10), new S(-20,-20),
        new S(-10,-10), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(-10,-10),
        new S(-10,-10), new S(  0,  0), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S(-10,-10),
        new S( -5, -5), new S(  0,  0), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S( -5, -5),
        new S(  0,  0), new S(  0,  0), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S( -5, -5),
        new S(-10,-10), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  5,  5), new S(  0,  0), new S(-10,-10),
        new S(-10,-10), new S(  0,  0), new S(  5,  5), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(  0,  0), new S(-10,-10),
        new S(-20,-20), new S(-10,-10), new S(-10,-10), new S( -5, -5), new S( -5, -5), new S(-10,-10), new S(-10,-10), new S(-20,-20)
    };

    // KingStart & KingEnd -> King
    public static readonly S[] King = 
    {
        new S(-80, -20), new S(-70, -10), new S(-70, -10), new S(-70, -10), new S(-70, -10), new S(-70, -10), new S(-70, -10), new S(-80, -20), 
        new S(-60,  -5), new S(-60,   0), new S(-60,   5), new S(-60,   5), new S(-60,   5), new S(-60,   5), new S(-60,   0), new S(-60,  -5), 
        new S(-40, -10), new S(-50,  -5), new S(-50,  20), new S(-60,  30), new S(-60,  30), new S(-50,  20), new S(-50,  -5), new S(-40, -10), 
        new S(-30, -15), new S(-40, -10), new S(-40,  35), new S(-50,  45), new S(-50,  45), new S(-40,  35), new S(-40, -10), new S(-30, -15), 
        new S(-20, -20), new S(-30, -15), new S(-30,  30), new S(-40,  40), new S(-40,  40), new S(-30,  30), new S(-30, -15), new S(-20, -20), 
        new S(-10, -25), new S(-20, -20), new S(-20,  20), new S(-20,  25), new S(-20,  25), new S(-20,  20), new S(-20, -20), new S(-10, -25), 
        new S( 20, -30), new S( 20, -25), new S( -5,   0), new S( -5,   0), new S( -5,   0), new S( -5,   0), new S( 20, -25), new S( 20, -30), 
        new S( 20, -50), new S( 30, -30), new S( 10, -30), new S(  0, -30), new S(  0, -30), new S( 10, -30), new S( 30, -30), new S( 20, -50)
    };

    public static readonly S[][] Tables = [
        [],
        Pawns,
        Knights,
        Bishops,
        Rooks,
        Queens,
        King
    ];

    public static int ReadTableFromPiece(Piece pieceType, Square square, bool white)
    {
        return Tables[pieceType][white ? SquareHelper.FlipRank(square) : square][0];
    }
}
