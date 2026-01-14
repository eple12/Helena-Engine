namespace H.Core;

using System;
using H.Program;

public class Board
{
    public BoardState State;
    private Piece[] Position = new Piece[64];

    // Stacking board data for unmaking moves will be implemented soon
    private Stack<BoardState> StateStack = new(20);

    // [Color]
    public Square[] KingSquares = new Square[2];

    // Bitboards
    // [Color]
    public BitboardSet[] BitboardSets = new BitboardSet[2];

    public MoveGen MoveGenerator;

    public Board()
    {
        MoveGenerator = new MoveGen(this);

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
            lastCaptured: PieceHelper.NONE,
            key: 0
        );
        // Empty board
        for (int i = 0; i < 64; i++)
        {
            Position[i] = PieceHelper.NONE;
        }

        StateStack = new();

        KingSquares[0] = SquareHelper.INVALID_SQUARE;
        KingSquares[1] = SquareHelper.INVALID_SQUARE;

        BitboardSets[0] = new BitboardSet();
        BitboardSets[1] = new BitboardSet();
    }

    public bool InCheck()
    {
        return State.Checkers != 0;
    }

// region Move Making
    // Input move MUST contain correct flag data and of course, start and target square as well
    // So for the user input(after position fen <fen> moves ...) we need to find the correct move from the legal moves list
    // (DO NOT CREATE A NEW MOVE OBJECT FROM START/TARGET SQUARE)
    public void MakeMove(Move move)
    {
        Color movingColor = PieceHelper.GetColor(State.SideToMove);
        Color enemyColor = PieceHelper.GetColor(!State.SideToMove);

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

        if (targetedPiece != PieceHelper.NONE)
        {
            State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(targetedPiece), target];
        }
        State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(movingPiece), start];
        State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(movingPiece), target];
    
        ushort flag = move.Flag;

        // Update King Square
        if (movingPieceType == PieceHelper.KING)
        {
            KingSquares[movingColor] = target;
        }

        // Basic bitboard update
        BitboardSets[movingColor].ToggleSquare(movingPieceType, start, target);
        
        if (targetedPieceType != PieceHelper.NONE)
        {
            BitboardSets[enemyColor].ToggleSquare(targetedPieceType, target);
        }

        if (State.EnPassantSquare != SquareHelper.INVALID_SQUARE)
        {
            State.Key ^= Zobrist.EP[SquareHelper.GetFile(State.EnPassantSquare)];
        }

        // En passant Square
        if (flag == MoveFlag.PawnTwo)
        {
            // The stm hasn't been switched yet.
            // If white pawn was pushed two squares forward, then it is the square right above the starting square
            // If black pawn was pushed two squares forward, then it is the square right below the starting square
            // (From White's perspective)
            State.EnPassantSquare = (Square) (start + (State.SideToMove ? 8 : -8));
            State.Key ^= Zobrist.EP[SquareHelper.GetFile(State.EnPassantSquare)];
        }
        else
        {
            State.EnPassantSquare = SquareHelper.INVALID_SQUARE;
        }

        ushort oldCastling = State.CastlingRights;
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

        if (State.CastlingRights != oldCastling)
        {
            State.Key ^= Zobrist.Castling[oldCastling];
            State.Key ^= Zobrist.Castling[State.CastlingRights];
        }

        // En passant
        if (flag == MoveFlag.EP)
        {
            Square epCaptured = (Square) (target + (State.SideToMove ? -8 : 8));
            Position[epCaptured] = PieceHelper.NONE;

            BitboardSets[enemyColor].ToggleSquare(PieceHelper.PAWN, epCaptured);

            State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(PieceHelper.PAWN, enemyColor), epCaptured];
        }
        // Castling
        else if (MoveFlag.IsCastling(flag))
        {
            Square rookStart = (Square) (target + (flag == MoveFlag.KCastling ? 1 : -2));
            Square rookTarget = (Square) (target + (flag == MoveFlag.KCastling ? -1 : 1));

            Position[rookTarget] = Position[rookStart];
            Position[rookStart] = PieceHelper.NONE;

            BitboardSets[movingColor].ToggleSquare(PieceHelper.ROOK, rookStart, rookTarget);

            int rookPieceIndex = PieceHelper.GetPieceIndex(PieceHelper.ROOK, movingColor);
            State.Key ^= Zobrist.Pieces[rookPieceIndex, rookStart];
            State.Key ^= Zobrist.Pieces[rookPieceIndex, rookTarget];
        }
        // Promotion
        else if (MoveFlag.IsPromotion(flag))
        {
            PieceType promType = MoveFlag.GetPromType(flag);
            Position[target] = PieceHelper.Make(promType, movingColor);

            BitboardSets[movingColor].ToggleSquare(PieceHelper.PAWN, target); // Remove the pawn on the promotion square
            BitboardSets[movingColor].ToggleSquare(promType, target); // Add the promoted piece

            State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(PieceHelper.PAWN, movingColor), target];
            State.Key ^= Zobrist.Pieces[PieceHelper.GetPieceIndex(promType, movingColor), target];
        }

        State.SideToMove = !State.SideToMove;
        State.Key ^= Zobrist.STM;

        // Increment or reset half move counter
        if (MoveFlag.IsCapture(flag) || movingPieceType == PieceHelper.PAWN)
        {
            State.HalfmoveClock = 0;
        }
        else
        {
            State.HalfmoveClock++;
        }

        CalculatePinData();
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
        PieceType movingPieceType = PieceHelper.GetPieceType(movingPiece);
        State = StateStack.Pop();

        Color movingColor = PieceHelper.GetColor(State.SideToMove);
        Color enemyColor = PieceHelper.GetColor(!State.SideToMove);

        Position[start] = movingPiece;
        Position[target] = State.LastCaptured;

        // Update King Square
        if (movingPieceType == PieceHelper.KING)
        {
            KingSquares[movingColor] = start;
        }

        // Basic bitboard update
        // Promotion is a special case
        if (!MoveFlag.IsPromotion(flag))
        {
            BitboardSets[movingColor].ToggleSquare(movingPieceType, target, start);

            if (State.LastCaptured != PieceHelper.NONE)
            {
                BitboardSets[enemyColor].ToggleSquare(PieceHelper.GetPieceType(State.LastCaptured), target);
            }
        }

        // Restore a pawn if the move was an en passant
        if (flag == MoveFlag.EP)
        {
            Square epCaptured = (Square) (target + (State.SideToMove ? -8 : 8));
            // Place an enemy pawn
            Position[epCaptured] = PieceHelper.Make(PieceHelper.PAWN, enemyColor);

            BitboardSets[enemyColor].ToggleSquare(PieceHelper.PAWN, epCaptured);
        }
        // Restore a rook if the move was a castling
        else if (MoveFlag.IsCastling(flag))
        {
            Square rookOriginal = (Square) (target + (flag == MoveFlag.KCastling ? 1 : -2));
            Square rookCurrent = (Square) (target + (flag == MoveFlag.KCastling ? -1 : 1));

            Position[rookOriginal] = Position[rookCurrent];
            Position[rookCurrent] = PieceHelper.NONE;

            BitboardSets[movingColor].ToggleSquare(PieceHelper.ROOK, rookOriginal, rookCurrent);
        }
        // Replace the piece with a pawn if the move was a promotion
        else if (MoveFlag.IsPromotion(flag))
        {
            // Since the promoted piece is back in the starting square, I need to replace the piece in starting square with a pawn
            Position[start] = PieceHelper.Make(PieceHelper.PAWN, movingColor);

            PieceType promType = MoveFlag.GetPromType(flag);
            BitboardSets[movingColor].ToggleSquare(promType, target);
            BitboardSets[movingColor].ToggleSquare(PieceHelper.PAWN, start);

            if (State.LastCaptured != PieceHelper.NONE)
            {
                BitboardSets[enemyColor].ToggleSquare(PieceHelper.GetPieceType(State.LastCaptured), target);
            }
        }
    }

// region Repetition and Draw Rules

    /// <summary>
    /// Checks if the current position is a draw due to the 50-move rule.
    /// A draw can be claimed if 50 full moves (100 half-moves) have been made
    /// by each player without a capture or a pawn move.
    /// </summary>
    public bool IsFiftyMoveDraw()
    {
        return State.HalfmoveClock >= 100;
    }

    /// <summary>
    /// Checks if the current position is a repetition.
    /// A position is considered a repetition if it has occurred at least once before
    /// in the sequence of moves since the last irreversible move (capture or pawn move).
    /// This method uses the Zobrist key of the board states to check for equality.
    /// </summary>
    public bool IsRepetition()
    {
        // We only need to check the history of positions since the last irreversible move.
        // The half-move clock tracks the number of plies since such a move.
        // We can't have a repetition if the clock or the history stack is empty.
        if (StateStack.Count == 0 || State.HalfmoveClock == 0)
        {
            return false;
        }

        ulong currentKey = State.Key;
        int pliesToLookBack = State.HalfmoveClock;

        // The StateStack (System.Collections.Generic.Stack) is iterated 
        // from the most recently added item to the oldest.
        int count = 0;
        foreach (BoardState pastState in StateStack)
        {
            // We only need to look as far back as the number of reversible moves made.
            if (count >= pliesToLookBack)
            {
                break;
            }

            // If we find a past state with the same Zobrist key, it's a repetition.
            if (pastState.Key == currentKey)
            {
                return true;
            }
            count++;
        }

        return false;
    }

    /// <summary>
    /// Checks if the position is a draw due to insufficient material.
    /// This occurs when neither player has enough material to force a checkmate.
    /// </summary>
    public bool IsInsufficientMaterial()
    {
        // If there are any pawns, rooks, or queens, it is not a draw.
        if (BitboardSets[0][PieceHelper.PAWN] != 0 || BitboardSets[1][PieceHelper.PAWN] != 0) return false;
        if (BitboardSets[0][PieceHelper.ROOK] != 0 || BitboardSets[1][PieceHelper.ROOK] != 0) return false;
        if (BitboardSets[0][PieceHelper.QUEEN] != 0 || BitboardSets[1][PieceHelper.QUEEN] != 0) return false;

        // At this point, only kings, knights, and bishops can be on the board.
        int numWhiteKnights = BitboardSets[0][PieceHelper.KNIGHT].Count();
        int numWhiteBishops = BitboardSets[0][PieceHelper.BISHOP].Count();
        int numWhiteMinors = numWhiteKnights + numWhiteBishops;

        int numBlackKnights = BitboardSets[1][PieceHelper.KNIGHT].Count();
        int numBlackBishops = BitboardSets[1][PieceHelper.BISHOP].Count();
        int numBlackMinors = numBlackKnights + numBlackBishops;

        // Rule: If both sides have no more than one minor piece, it's likely a draw.
        if (numWhiteMinors <= 1 && numBlackMinors <= 1)
        {
            // Case: King + Bishop vs King + Bishop
            if (numWhiteBishops == 1 && numBlackBishops == 1)
            {
                ulong whiteBishopBB = BitboardSets[0].Indexed(PieceHelper.BISHOP);
                ulong blackBishopBB = BitboardSets[1].Indexed(PieceHelper.BISHOP);

                // Get the square of each bishop. Assuming a helper for LSB.
                Square whiteBishopSq = (Square) whiteBishopBB.PopLSB();
                Square blackBishopSq = (Square) blackBishopBB.PopLSB();

                // If bishops are on the same color squares, it's a draw.
                if (SquareHelper.IsLightSquare(whiteBishopSq) == SquareHelper.IsLightSquare(blackBishopSq))
                {
                    return true;
                }
            }
            else
            {
                // Covers K vs K, K+N vs K, K+B vs K, K+N vs K+N, K+N vs K+B.
                // All of these are draws.
                return true;
            }
        }

        return false;
    }

// endregion
// endregion

    // Calculate Pin & Checker data for MoveGen & SEE
    void CalculatePinData()
    {
        Color us = PieceHelper.GetColor(State.SideToMove);
        Color them = PieceHelper.GetColor(!State.SideToMove);

        Square ksq = KingSquares[us];
        Bitboard notKsq = ~(1ul << ksq);

        Bitboard occupied = BitboardSets[us].All | BitboardSets[them].All;
        Bitboard queens = BitboardSets[them][PieceHelper.QUEEN];
        Bitboard rookLike = BitboardSets[them][PieceHelper.ROOK] | queens;
        Bitboard bishopLike = BitboardSets[them][PieceHelper.BISHOP] | queens;

        // Pawn & Knight checkers
        Bitboard checkers = (
            (Bits.PawnAttacks[us][ksq] & BitboardSets[them][PieceHelper.PAWN]) |
            (Bits.KnightMovement[ksq] & BitboardSets[them][PieceHelper.KNIGHT])
        );

        Bitboard checkRayBitmask = checkers;
        Bitboard pinned = 0;

        int startDirIndex = 0;
        int endDirIndex = 8;

        // Skip direction check (Pin / Check)
        // There are no enemy queens
        if (queens == 0)
        {
            startDirIndex = rookLike != 0 ? 0 : 4;
            endDirIndex = bishopLike != 0 ? 8 : 4;
        }

        for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++)
        {
            bool isDiagonal = dirIndex >= 4;
            Bitboard slider = isDiagonal ? bishopLike : rookLike;

            Bitboard dirRay = Bits.DirRayMasks[ksq][dirIndex];
            if ((dirRay & slider) == 0)
            {
                continue;
            }

            Bitboard blockers = dirRay & occupied & notKsq;

            Square firstBlocker = BitboardHelper.FirstBlocker(blockers, dirIndex);
            if (PieceHelper.IsColor(At(firstBlocker), them)) // Enemy piece: checker
            {
                if (slider.Contains(firstBlocker)) // Slider: checker
                {
                    checkers |= 1ul << firstBlocker;
                    checkRayBitmask |= Bits.BetweenMasks[ksq + Bits.DirectionOffsets[dirIndex]][firstBlocker];
                } // else: doesn't matter, this piece is blocking the ray and it is not a checker
                
                continue;
            }
            else // Friendly piece: might be pinned
            {
                blockers.ToggleSquare(firstBlocker); // Remove the first blocker
                Square secondBlocker = BitboardHelper.FirstBlocker(blockers, dirIndex);

                if (PieceHelper.IsColor(At(secondBlocker), them)) // Might be a pinner
                {
                    if (slider.Contains(secondBlocker)) // Slider: pinner
                    {
                        // Does not include the king. In the direction, king + 1 ~ pinner
                        pinned.SetSquare(firstBlocker);

                        continue;
                    } // else: not a pinner
                } // else: two friendly pieces

                continue;
            }
        }

        if (checkers == 0)
        {
            checkRayBitmask = ulong.MaxValue;
        }

        // Save them
        State.Checkers = checkers;
        State.CheckRay = checkRayBitmask;
        State.Pinned = pinned;
    }

    // Assume that the FEN is valid for "performance".. although it will NOT be used in the search
    // (I just want to keep things simple for now)
    public void LoadPositionFromFEN(string fen)
    {
        ClearBoard();

        ReadFenString();

        State.Key = Zobrist.GetZobristKey(this);

        // Update Checkers and Pins
        CalculatePinData();

        void ReadFenString()
        {
            fen = fen.Trim();

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

                    if (type == PieceHelper.KING)
                    {
                        KingSquares[color] = square;
                    }

                    // Update Bitboards
                    if (type != PieceHelper.NONE)
                    {
                        BitboardSets[color].ToggleSquare(type, square);
                    }
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
    }

    public void PrintMoves()
    {
        Move[] moves = MoveGenerator.GenerateMoves().ToArray();
        Logger.LogLine($"Count: {moves.Length}");
        Logger.LogLine(string.Join(' ', moves.Select(m => m.Notation)));
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

    public void Test()
    {
        CalculatePinData();
        // MoveList moves = MoveGenerator.GenerateMoves();
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

    public Bitboard Checkers;
    public Bitboard CheckRay;
    public Bitboard Pinned;

    public ulong Key;

    public BoardState(bool sideToMove, ushort castlingRights, byte enPassantSquare, 
        ushort halfmoveClock = 0, Piece lastCaptured = PieceHelper.NONE, ulong key = 0,
        Bitboard checkers = 0, Bitboard checkRay = 0, Bitboard pinned = 0)
    {
        SideToMove = sideToMove;
        CastlingRights = castlingRights;
        EnPassantSquare = enPassantSquare;
        HalfmoveClock = halfmoveClock;
        LastCaptured = lastCaptured;
        Key = key;

        Checkers = checkers;
        CheckRay = checkRay;
        Pinned = pinned;
    }

    public void Print()
    {
        Logger.LogLine($"Side: {(SideToMove ? "White" : "Black")} | EP: {SquareHelper.ToString(EnPassantSquare)} | Castling: {(NoCastling ? "-" : ((WKCastling ? "K" : "") + (WQCastling ? "Q" : "") + (BKCastling ? "k" : "") + (BQCastling ? "q" : "")))} | HalfClock: {HalfmoveClock} | Key: {Key}");
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
