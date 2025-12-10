namespace H.Core;

using System;
using H.Program;

public class Board
{
    public BoardState State;
    private Piece[] Position = new Piece[64];

    // Stacking board data for unmaking moves will be implemented soon
    private Stack<BoardState> StateStack = new(20);

    public Board()
    {
        Initialize();
    }

    void Initialize()
    {
        ClearBoard();
        WarmUp();
    }

    void WarmUp()
    {
        // Just call some functions
        // Not implemented
    }

    // Clear the entire board.. so that it is absolutely empty
    void ClearBoard()
    {
        State = new BoardState(
            sideToMove: true,
            castlingRights: BoardState.NoCastlingBits,
            enPassantSquare: SquareHelper.INVALID_SQUARE,
            halfmoveClock: 0,
            lastCaptured: PieceHelper.NONE
        );
        // Empty board
        for (int i = 0; i < 64; i++)
        {
            Position[i] = PieceHelper.NONE;
        }

        StateStack = new();
    }

// region Move Making
    // Input move MUST contain correct flag data and of course, start and target square as well
    // So for the user input(after position fen <fen> moves ...) we need to find the correct move from the legal moves list
    // (DO NOT CREATE A NEW MOVE OBJECT FROM START/TARGET SQUARE)
    public void MakeMove(Move move)
    {
        Square start = move.Start;
        Square target = move.Target;

        Piece movingPiece = At(start);
        Piece targetedPiece = At(target);

        PieceType movingPieceType = PieceHelper.GetPieceType(movingPiece);
        PieceType targetedPieceType = PieceHelper.GetPieceType(targetedPiece);

        // Store the board state for unmaking a move before making it
        // When unmaking a move, pop out the last state and use LastCaptured to restore a captured piece
        State.LastCaptured = targetedPiece;
        BoardState beforeState = State;
        StateStack.Push(beforeState);

        Position[target] = movingPiece;
        Position[start] = PieceHelper.NONE;

        ushort flag = move.Flag;

        // En passant Square
        if (flag == MoveFlag.PawnTwo)
        {
            // The stm hasn't been switched yet.
            // If white pawn was pushed two squares forward, then it is the square right above the starting square
            // If black pawn was pushed two squares forward, then it is the square right below the starting square
            // (From White's perspective)
            State.EnPassantSquare = (Square) (start + (State.SideToMove ? 8 : -8));
        }
        else
        {
            State.EnPassantSquare = SquareHelper.INVALID_SQUARE;
        }

        // Castling Rights
        if (movingPieceType == PieceHelper.KING)
        {
            if (State.SideToMove)
            {
                State.SetWKCastling(false);
                State.SetWQCastling(false);
            }
            else
            {
                State.SetBKCastling(false);
                State.SetBQCastling(false);
            }
        }
        else if (movingPieceType == PieceHelper.ROOK)
        {
            if (State.SideToMove) // White
            {
                if (State.WKCastling && start == SquareHelper.H1)
                {
                    State.SetWKCastling(false);
                }
                else if (State.WQCastling && start == SquareHelper.A1)
                {
                    State.SetWQCastling(false);
                }
            }
            else // Black
            {
                if (State.BKCastling && start == SquareHelper.H8)
                {
                    State.SetBKCastling(false);
                }
                else if (State.BQCastling && start == SquareHelper.A8)
                {
                    State.SetBQCastling(false);
                }
            }
        }

        // If a rook is captured, the castling right might be changed
        if (targetedPieceType == PieceHelper.ROOK)
        {
            if (State.SideToMove) // Black rook captured
            {
                if (State.BKCastling && target == SquareHelper.H8)
                {
                    State.SetBKCastling(false);
                }
                else if (State.BQCastling && target == SquareHelper.A8)
                {
                    State.SetBQCastling(false);
                }
            }
            else // White rook captured
            {
                if (State.WKCastling && target == SquareHelper.H1)
                {
                    State.SetWKCastling(false);
                }
                else if (State.WQCastling && target == SquareHelper.A1)
                {
                    State.SetWQCastling(false);
                }
            }
        }

        // En passant
        if (flag == MoveFlag.EP)
        {
            Square epCaptured = (Square) (target + (State.SideToMove ? -8 : 8));
            Position[epCaptured] = PieceHelper.NONE;
        }
        // Castling
        else if (MoveFlag.IsCastling(flag))
        {
            Square rookStart = (Square) (target + (flag == MoveFlag.KCastling ? 1 : -2));
            Square rookTarget = (Square) (target + (flag == MoveFlag.KCastling ? -1 : 1));

            Position[rookTarget] = Position[rookStart];
            Position[rookStart] = PieceHelper.NONE;
        }
        // Promotion
        else if (MoveFlag.IsPromotion(flag))
        {
            PieceType promType = MoveFlag.GetPromType(flag);
            Position[target] = PieceHelper.Make(promType, PieceHelper.GetColor(State.SideToMove));
        }

        State.SideToMove = !State.SideToMove;

        // Increment or reset half move counter
        if (MoveFlag.IsCapture(flag) || movingPieceType == PieceHelper.PAWN)
        {
            State.HalfmoveClock = 0;
        }
        else
        {
            State.HalfmoveClock++;
        }
    }

    public void UnmakeMove(Move move)
    {
        if (StateStack.Count == 0)
        {
            Logger.LogLine($"Error: Cannot unmake the move {move.Notation} since the State Stack is empty.");
            return;
        }

        Square start = move.Start;
        Square target = move.Target;
        ushort flag = move.Flag;

        Piece movingPiece = At(target);
        State = StateStack.Pop();

        Position[start] = movingPiece;
        Position[target] = State.LastCaptured;

        // Restore a pawn if the move was an en passant
        if (flag == MoveFlag.EP)
        {
            Square epCaptured = (Square) (target + (State.SideToMove ? -8 : 8));
            // Place an enemy pawn
            Position[epCaptured] = PieceHelper.Make(PieceHelper.PAWN, PieceHelper.GetColor(!State.SideToMove));
        }
        // Restore a rook if the move was a castling
        else if (MoveFlag.IsCastling(flag))
        {
            Square rookOriginal = (Square) (target + (flag == MoveFlag.KCastling ? 1 : -2));
            Square rookCurrent = (Square) (target + (flag == MoveFlag.KCastling ? -1 : 1));

            Position[rookOriginal] = Position[rookCurrent];
            Position[rookCurrent] = PieceHelper.NONE;
        }
        // Replace the piece with a pawn if the move was a promotion
        else if (MoveFlag.IsPromotion(flag))
        {
            // Since the promoted piece is back in the starting square, I need to replace the piece in starting square with a pawn
            Position[start] = PieceHelper.Make(PieceHelper.PAWN, PieceHelper.GetColor(State.SideToMove));
        }
    }
// endregion

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

    public void PrintLargeBoard()
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            Logger.LogLine("  +---+---+---+---+---+---+---+---+");
            Logger.Log($"{rank + 1} |");
            for (int file = 0; file < 8; file++)
            {
                Logger.Log($" {PieceHelper.ToChar(Position[SquareHelper.GetSquare(file, rank)])} |");
            }
            Logger.Log('\n');
        }

        Logger.LogLine("  +---+---+---+---+---+---+---+---+");
        Logger.LogLine("    A   B   C   D   E   F   G   H ");
        State.Print();
    }

    // The piece at the square
    public Piece At(Square square)
    {
        return Position[square];
    }
}

public struct BoardState
{
    public bool SideToMove; // true for White
    public ushort CastlingRights; // Assume that only 4 least significant bits are used, and the rest are 0 for performance
    public byte EnPassantSquare;
    public ushort HalfmoveClock;
    public Piece LastCaptured;

    public BoardState(bool sideToMove, ushort castlingRights, byte enPassantSquare, 
        ushort halfmoveClock = 0, Piece lastCaptured = PieceHelper.NONE)
    {
        SideToMove = sideToMove;
        CastlingRights = castlingRights;
        EnPassantSquare = enPassantSquare;
        HalfmoveClock = halfmoveClock;
        LastCaptured = lastCaptured;
    }

    public void Print()
    {
        Logger.LogLine($"Side: {(SideToMove ? "White" : "Black")} | EP: {SquareHelper.ToString(EnPassantSquare)} | Castling: {(NoCastling ? "-" : ((WKCastling ? "K" : "") + (WQCastling ? "Q" : "") + (BKCastling ? "k" : "") + (BQCastling ? "q" : "")))} | HalfClock: {HalfmoveClock}");
    }

    // Castling rights expressions & constants
    // WK WQ BK BQ
    public readonly bool NoCastling => CastlingRights == NoCastlingBits;
    public readonly bool AllCastling => CastlingRights == AllCastlingBits;
    public readonly bool WKCastling => (CastlingRights & WKCastlingMask) != 0;
    public readonly bool WQCastling => (CastlingRights & WQCastlingMask) != 0;
    public readonly bool BKCastling => (CastlingRights & BKCastlingMask) != 0;
    public readonly bool BQCastling => (CastlingRights & BQCastlingMask) != 0;

    public const ushort NoCastlingBits = 0b0000;
    public const ushort AllCastlingBits = 0b1111;
    public const ushort WKCastlingMask = 0b1000;
    public const ushort WQCastlingMask = 0b0100;
    public const ushort BKCastlingMask = 0b0010;
    public const ushort BQCastlingMask = 0b0001;

    public void SetWKCastling(bool flag)
    {
        CastlingRights = flag ? (ushort) (CastlingRights | WKCastlingMask) : (ushort) (CastlingRights & ~WKCastlingMask);
    }
    public void SetWQCastling(bool flag)
    {
        CastlingRights = flag ? (ushort) (CastlingRights | WQCastlingMask) : (ushort) (CastlingRights & ~WQCastlingMask);
    }
    public void SetBKCastling(bool flag)
    {
        CastlingRights = flag ? (ushort) (CastlingRights | BKCastlingMask) : (ushort) (CastlingRights & ~BKCastlingMask);
    }
    public void SetBQCastling(bool flag)
    {
        CastlingRights = flag ? (ushort) (CastlingRights | BQCastlingMask) : (ushort) (CastlingRights & ~BQCastlingMask);
    }
}
