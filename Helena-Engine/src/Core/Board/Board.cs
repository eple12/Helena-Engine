namespace H.Core;

using System;

public class Board
{
    public BoardState State;
    private Piece[] Position = new Piece[64];

    public Board()
    {
        Initialize();

        // PrintLargeBoard();
    }

    void Initialize()
    {
        ClearBoard();
    }

    // Clear the entire board.. so that it is absolutely empty
    void ClearBoard()
    {
        State = new BoardState(
            sideToMove: true,
            castlingRights: BoardState.NoCastlingBits,
            enPassantSquare: SquareHelper.INVALID_SQUARE,
            halfmoveClock: 0
        );
        // Empty board
        for (int i = 0; i < 64; i++)
        {
            Position[i] = PieceHelper.NONE;
        }
    }

    // Assume that the FEN is valid for "performance".. although it will NOT be used in the search
    // (I just want to keep things simple for now)
    public void LoadPositionFromFEN(string fen)
    {
        ClearBoard();

        // FEN parsing
        string[] parts = fen.Split(' ');

        string pieces = parts[0];
        int rank = 7;
        int file = 0;

        foreach (char c in pieces)
        {
            if (c == '/')
            {
                rank--;
                file = 0;
                continue;
            }

            if (char.IsDigit(c))
            {
                int emptySquares = c - '0'; // character type digit to integer; oldest trick in the book
                file += emptySquares;
            }
            else
            {
                Color color = PieceHelper.GetColor(char.IsUpper(c));
                PieceType type = c.ToString().ToLower() switch
                {
                    "p" => PieceHelper.PAWN,
                    "n" => PieceHelper.KNIGHT,
                    "b" => PieceHelper.BISHOP,
                    "r" => PieceHelper.ROOK,
                    "q" => PieceHelper.QUEEN,
                    "k" => PieceHelper.KING,
                    _ => PieceHelper.NONE
                };

                Square square = SquareHelper.GetSquare(file, rank);
                Position[square] = PieceHelper.Make(type, color);
                file++;
            }
        }

        // stm stands for side to move btw
        string stm = parts[1];
        State.SideToMove = stm == "w";

        // Castling rights
        string castling = parts[2];
        ushort castlingRights = BoardState.NoCastlingBits;
        if (castling.Contains('K')) castlingRights |= BoardState.WKCastlingMask;
        if (castling.Contains('Q')) castlingRights |= BoardState.WQCastlingMask;
        if (castling.Contains('k')) castlingRights |= BoardState.BKCastlingMask;
        if (castling.Contains('q')) castlingRights |= BoardState.BQCastlingMask;
        State.CastlingRights = castlingRights;

        // Below are optional; so we check the FEN length
        if (parts.Length < 4)
        {
            return; // It's ok because we emptied the board, so EP square will be INVALID_SQUARE
        }

        // En passant square
        string enPassant = parts[3];
        if (enPassant == "-")
        {
            State.EnPassantSquare = SquareHelper.INVALID_SQUARE;
        }
        else
        {
            int fileEP = enPassant[0] - 'a';
            int rankEP = enPassant[1] - '1';
            State.EnPassantSquare = SquareHelper.GetSquare(fileEP, rankEP);
        }

        if (parts.Length < 5)
        {
            return;
        }

        // Halfmove clock
        State.HalfmoveClock = ushort.Parse(parts[4]);
    }

    // Running it the first time seems to have some costs
    public void PrintLargeBoard()
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            System.Console.WriteLine("  +---+---+---+---+---+---+---+---+");
            System.Console.Write($"{rank + 1} |");
            for (int file = 0; file < 8; file++)
            {
                System.Console.Write($" {PieceHelper.ToChar(Position[SquareHelper.GetSquare(file, rank)])} |");
            }
            System.Console.Write('\n');
        }

        System.Console.WriteLine("  +---+---+---+---+---+---+---+---+");
        System.Console.WriteLine("    A   B   C   D   E   F   G   H ");
        State.Print();
    }
}

public struct BoardState
{
    public bool SideToMove; // true for White
    public ushort CastlingRights; // Assume that only 4 least significant bits are used, and the rest are 0 for performance
    public byte EnPassantSquare;
    public ushort HalfmoveClock;

    public BoardState(bool sideToMove, ushort castlingRights, byte enPassantSquare, ushort halfmoveClock)
    {
        SideToMove = sideToMove;
        CastlingRights = castlingRights;
        EnPassantSquare = enPassantSquare;
        HalfmoveClock = halfmoveClock;
    }

    public void Print()
    {
        System.Console.WriteLine($"Side: {(SideToMove ? "White" : "Black")} | EP: {SquareHelper.ToString(EnPassantSquare)} | Castling: {(NoCastling ? "-" : ((WKCastling ? "K" : "") + (WQCastling ? "Q" : "") + (BKCastling ? "k" : "") + (BQCastling ? "q" : "")))} | HalfClock: {HalfmoveClock}");
    }

    // Castling rights
    // WK WQ BK BQ
    public bool NoCastling => CastlingRights == NoCastlingBits;
    public bool AllCastling => CastlingRights == AllCastlingBits;
    public bool WKCastling => (CastlingRights & WKCastlingMask) != 0;
    public bool WQCastling => (CastlingRights & WQCastlingMask) != 0;
    public bool BKCastling => (CastlingRights & BKCastlingMask) != 0;
    public bool BQCastling => (CastlingRights & BQCastlingMask) != 0;

    public const ushort NoCastlingBits = 0b0000;
    public const ushort AllCastlingBits = 0b1111;
    public const ushort WKCastlingMask = 0b1000;
    public const ushort WQCastlingMask = 0b0100;
    public const ushort BKCastlingMask = 0b0010;
    public const ushort BQCastlingMask = 0b0001;
}
