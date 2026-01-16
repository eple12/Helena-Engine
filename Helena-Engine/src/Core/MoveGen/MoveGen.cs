namespace H.Core;

/*
    Credit to Sebastian Lague: https://github.com/SebLague/Chess-Coding-Adventure
*/

// A MoveGen object will be attached to a Board
public class MoveGen
{
    // Board reference
    Board board;

    int MAX_MOVES => Constants.MAX_MOVES;

    // Field used in generation
    bool isWhiteToMove;
    Color friendlyColor;
    Color enemyColor;
    Square friendlyKingSquare;

    bool inCheck;
    bool inDoubleCheck;

    // If in check, this bitboard contains squares in line from checking piece up to king
    // If not in check, all bits are set to 1
    Bitboard checkRayBitmask;

    Bitboard pinned;
    Bitboard notPinned;
    Bitboard enemyAttackMapNoPawns;
    Bitboard enemyAttackMap;
    Bitboard enemyPawnAttackMap;
    Bitboard enemySlidingAttackMap;

    bool generateQuietMoves;
    int currMoveIdx = 0;

    Bitboard friendlyPieces;
    Bitboard enemyPieces;
    Bitboard allPieces;
    Bitboard emptySquares;
    Bitboard emptyOrEnemySquares;

    Bitboard enemyOrthoSliders;
    Bitboard enemyDiagSliders;

    // If only captures should be generated, this will have 1s only in positions of enemy pieces.
    // Otherwise it will have 1s everywhere.
    Bitboard moveTypeMask;

    public MoveGen(Board _board)
    {
        board = _board;
    }

    public bool InCheck()
    {
        return inCheck;
    }
    public Bitboard EnemyPawnAttackMap()
    {
        return enemyPawnAttackMap;
    }
    public Bitboard EnemyAttackMapNoPawn()
    {
        return enemyAttackMapNoPawns;
    }
    public Bitboard EnemyAttackMap()
    {
        return enemyAttackMap;
    }

    public MoveList GenerateMoves(bool capturesOnly = false)
    {
        MoveList moves = new Move[MAX_MOVES];

        generateQuietMoves = !capturesOnly;

        Initialize();

        Generate(ref moves);
        
        moves = moves[..currMoveIdx];
        return moves;
    }
    public MoveList GenerateMoves(ref MoveList moves, bool capturesOnly = false)
    {
        generateQuietMoves = !capturesOnly;

        Initialize();

        Generate(ref moves);
        
        moves = moves[..currMoveIdx];
        return moves;
    }

    void Generate(ref MoveList moves)
    {
        GenerateKingMoves(ref moves);

        if (inDoubleCheck)
        {
            return;
        }

        // Generate other moves
        GenerateSlidingMoves(ref moves);
        GenerateKnightMoves(ref moves);
        GeneratePawnMoves(ref moves);
    }

    void AddNormalMove(ref MoveList moves, Square start, Square target)
    {
        bool isCapture = enemyPieces.Contains(target);
        moves[currMoveIdx++] = new Move(start, target, isCapture ? MoveFlag.Capture : MoveFlag.None);
    }
    void AddMove(ref MoveList moves, Square start, Square target, ushort flag)
    {
        moves[currMoveIdx++] = new Move(start, target, flag);
    }

    void GeneratePawnMoves(ref MoveList moves)
    {
        int pushDir = isWhiteToMove ? 1 : -1; // If white, goes forward. Otherwise goes backward.
        int pushOffset = pushDir * 8;

        Bitboard pawns = board.BitboardSets[friendlyColor][PieceHelper.PAWN];
        Bitboard promotionRankMask = isWhiteToMove ? Bits.Rank8 : Bits.Rank1;

        Bitboard singlePush = BitboardHelper.Shift(pawns, pushOffset) & emptySquares;
        Bitboard pushPromotions = singlePush & promotionRankMask & checkRayBitmask;
    
        Bitboard captureEdgeFileMask = isWhiteToMove ? Bits.NotFileA : Bits.NotFileH;
        Bitboard captureEdgeFileMask2 = isWhiteToMove ? Bits.NotFileH : Bits.NotFileA;
        Bitboard captureA = BitboardHelper.Shift(pawns & captureEdgeFileMask, pushDir * 7) & enemyPieces;
        Bitboard captureB = BitboardHelper.Shift(pawns & captureEdgeFileMask2, pushDir * 9) & enemyPieces;

        Bitboard singlePushNoPromotions = singlePush & ~promotionRankMask & checkRayBitmask;

        Bitboard capturePromotionsA = captureA & promotionRankMask & checkRayBitmask;
        Bitboard capturePromotionsB = captureB & promotionRankMask & checkRayBitmask;

        captureA &= checkRayBitmask & ~promotionRankMask;
        captureB &= checkRayBitmask & ~promotionRankMask;

        if (generateQuietMoves)
        {
            while (singlePushNoPromotions != 0)
            {
                Square target = (Square) singlePushNoPromotions.PopLSB();
                Square start = (Square) (target - pushOffset);

                if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
                {
                    AddMove(ref moves, start, target, MoveFlag.None);
                }
            }

            Bitboard doublePushTargetRankMask = isWhiteToMove ? Bits.Ranks[3] : Bits.Ranks[4];
            Bitboard doublePush = BitboardHelper.Shift(singlePush, pushOffset) & emptySquares & doublePushTargetRankMask & checkRayBitmask;

            while (doublePush != 0)
            {
                Square target = (Square) doublePush.PopLSB();
                Square start = (Square) (target - 2 * pushOffset);

                if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
                {
                    AddMove(ref moves, start, target, MoveFlag.PawnTwo);
                }
            }
        }

        // Captures
        while (captureA != 0)
        {
            Square target = (Square) captureA.PopLSB();
            Square start = (Square) (target - pushDir * 7);

            if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
            {
                AddMove(ref moves, start, target, MoveFlag.Capture);
            }
        }
        while (captureB != 0)
        {
            Square target = (Square) captureB.PopLSB();
            Square start = (Square) (target - pushDir * 9);

            if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
            {
                AddMove(ref moves, start, target, MoveFlag.Capture);
            }
        }

        // Promotion
        while (pushPromotions != 0)
        {
            Square target = (Square) pushPromotions.PopLSB();
            Square start = (Square) (target - pushOffset);

            if (!IsPinned(start)) // No need to check for moving along the pin ray, since promotion means that the pawn reached the end of the board (there are no enemy pieces behind this pawn)
            {
                GeneratePromotions(ref moves, start, target, false);
            }
        }

        while (capturePromotionsA != 0)
        {
            Square target = (Square) capturePromotionsA.PopLSB();
            Square start = (Square) (target - pushDir * 7);

            if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
            {
                GeneratePromotions(ref moves, start, target, true);
            }
        }
        while (capturePromotionsB != 0)
        {
            Square target = (Square) capturePromotionsB.PopLSB();
            Square start = (Square) (target - pushDir * 9);

            if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
            {
                GeneratePromotions(ref moves, start, target, true);
            }
        }

        if (board.State.EnPassantSquare != SquareHelper.INVALID_SQUARE)
        {
            Square target = board.State.EnPassantSquare;
            Square capturedPawn = (Square) (target - pushOffset);

            if (checkRayBitmask.Contains(capturedPawn))
            {
                Bitboard possiblePawns = pawns & Bits.PawnAttacks[enemyColor][target];

                while (possiblePawns != 0)
                {
                    Square start = (Square) possiblePawns.PopLSB();
                    if (!IsPinned(start) || Bits.AlignMasks[start][friendlyKingSquare] == Bits.AlignMasks[target][friendlyKingSquare])
                    {
                        if (!InCheckAfterEP(start, target, capturedPawn))
                        {
                            AddMove(ref moves, start, target, MoveFlag.EP);
                        }
                    }
                }
            }
        }
    }

    bool InCheckAfterEP(Square start, Square target, Square captured)
    {
        Bitboard enemyOrtho = enemyOrthoSliders;

        if (enemyOrtho != 0)
        {
            Bitboard maskedBlockers = allPieces ^ ((1ul << captured) | (1ul << start) | (1ul << target));
            Bitboard rookAttacks = Magic.GetRookAttacks(friendlyKingSquare, maskedBlockers);

            return (rookAttacks & enemyOrtho) != 0;
        }

        return false;
    }

    void GeneratePromotions(ref MoveList moves, Square start, Square target, bool isCapture)
    {
        AddMove(ref moves, start, target, MoveFlag.GetPromFlag(PieceHelper.QUEEN, isCapture));

        if (generateQuietMoves)
        {
            AddMove(ref moves, start, target, MoveFlag.GetPromFlag(PieceHelper.ROOK, isCapture));
            AddMove(ref moves, start, target, MoveFlag.GetPromFlag(PieceHelper.BISHOP, isCapture));
            AddMove(ref moves, start, target, MoveFlag.GetPromFlag(PieceHelper.KNIGHT, isCapture));
        }
    }

    void GenerateKnightMoves(ref MoveList moves)
    {
        Bitboard knights = board.BitboardSets[friendlyColor][PieceHelper.KNIGHT] & notPinned;
        Bitboard moveMask = emptyOrEnemySquares & checkRayBitmask & moveTypeMask;

        while (knights != 0)
        {
            Square knightSquare = (Square) knights.PopLSB();
            Bitboard moveSquares = Bits.KnightMovement[knightSquare] & moveMask;

            while (moveSquares != 0)
            {
                Square target = (Square) moveSquares.PopLSB();
                AddNormalMove(ref moves, knightSquare, target);
            }
        }
    }

    void GenerateSlidingMoves(ref MoveList moves)
    {
        // Empty or Enemy & Resolves check (& All/Only Capture)
        Bitboard moveMask = emptyOrEnemySquares & checkRayBitmask & moveTypeMask;

        Bitboard orthogonalSliders = board.BitboardSets[friendlyColor][PieceHelper.ROOK] | board.BitboardSets[friendlyColor][PieceHelper.QUEEN];
        Bitboard diagonalSliders = board.BitboardSets[friendlyColor][PieceHelper.BISHOP] | board.BitboardSets[friendlyColor][PieceHelper.QUEEN];

        if (inCheck) // If in check, pinned sliders cannot resolve the check
        {
            orthogonalSliders &= notPinned;
            diagonalSliders &= notPinned;
        }

        while (orthogonalSliders != 0)
        {
            Square start = (Square) orthogonalSliders.PopLSB();
            Bitboard moveSquares = Magic.GetRookAttacks(start, allPieces) & moveMask;

            if (IsPinned(start))
            {
                moveSquares &= Bits.AlignMasks[friendlyKingSquare][start];
            }

            while (moveSquares != 0)
            {
                Square target = (Square) moveSquares.PopLSB();
                AddNormalMove(ref moves, start, target);
            }
        }
        while (diagonalSliders != 0)
        {
            Square start = (Square) diagonalSliders.PopLSB();
            Bitboard moveSquares = Magic.GetBishopAttacks(start, allPieces) & moveMask;

            if (IsPinned(start))
            {
                moveSquares &= Bits.AlignMasks[friendlyKingSquare][start];
            }

            while (moveSquares != 0)
            {
                Square target = (Square) moveSquares.PopLSB();
                AddNormalMove(ref moves, start, target);
            }
        }
    }

    void GenerateKingMoves(ref MoveList moves)
    {
        Bitboard legalMask = ~(enemyAttackMap | friendlyPieces);
        Bitboard kingMoves = Bits.KingMovement[friendlyKingSquare] & legalMask & moveTypeMask;
        while (kingMoves != 0)
        {
            Square target = (Square) kingMoves.PopLSB();
            AddNormalMove(ref moves, friendlyKingSquare, target);
        }

        // Castling
        if (!inCheck && generateQuietMoves)
        {
            Bitboard castlingBlockers = enemyAttackMap | allPieces;
            
            bool kCastlingRight = isWhiteToMove ? board.State.WKCastling : board.State.BKCastling;
            bool qCastlingRight = isWhiteToMove ? board.State.WQCastling : board.State.BQCastling;

            if (kCastlingRight)
            {
                Bitboard castlingMask = Bits.KCastlingWay[friendlyColor];
                if ((castlingMask & castlingBlockers) == 0)
                {
                    Square target = (Square) (friendlyKingSquare + 2);
                    AddMove(ref moves, friendlyKingSquare, target, MoveFlag.KCastling);
                }
            }
            if (qCastlingRight)
            {
                Bitboard castlingMask = Bits.QCastlingWay[friendlyColor];
                Bitboard blockingMask = Bits.QCastling3[friendlyColor];

                if (((castlingMask & castlingBlockers) | (blockingMask & allPieces)) == 0)
                {
                    Square target = (Square) (friendlyKingSquare - 2);
                    AddMove(ref moves, friendlyKingSquare, target, MoveFlag.QCastling);
                }
            }
        }
    }

    bool IsPinned(Square sq)
    {
        return pinned.Contains(sq);
    }

    // Refresh the class every execution
    public void Initialize()
    {
        currMoveIdx = 0;
        inCheck = board.State.Checkers != 0;
        inDoubleCheck = board.State.Checkers.MoreThanOne();
        checkRayBitmask = board.State.CheckRay;
        pinned = board.State.Pinned;
        notPinned = ~pinned;

        isWhiteToMove = board.State.SideToMove;
        friendlyColor = PieceHelper.GetColor(isWhiteToMove);
        enemyColor = PieceHelper.GetColor(!isWhiteToMove);

        friendlyKingSquare = board.KingSquares[friendlyColor];

        friendlyPieces = board.BitboardSets[friendlyColor].All;
        enemyPieces = board.BitboardSets[enemyColor].All;

        // I don't keep track of "ALL" pieces on the board
        // I do have the separate "White ALL" and "Black ALL" bitboards though
        // Let's keep things simple and just OR them here
        // This surely won't affect the "performance" that much.. right?
        allPieces = friendlyPieces | enemyPieces;
        emptySquares = ~allPieces;
        emptyOrEnemySquares = emptySquares | enemyPieces;

        enemyOrthoSliders = board.BitboardSets[enemyColor][PieceHelper.ROOK] | board.BitboardSets[enemyColor][PieceHelper.QUEEN];
        enemyDiagSliders = board.BitboardSets[enemyColor][PieceHelper.BISHOP] | board.BitboardSets[enemyColor][PieceHelper.QUEEN];

        // In QSearch, we only find capturing moves (target square must have an enemy piece)
        // So we will AND this mask below
        moveTypeMask = generateQuietMoves ? Bitboard.MaxValue : enemyPieces;

        CalculateAttackData();
    }

    void GenerateSlidingAttackMap()
    {
        enemySlidingAttackMap = 0;

        UpdateSlideAttack(enemyOrthoSliders, true);
        UpdateSlideAttack(enemyDiagSliders, false);

        void UpdateSlideAttack(ulong pieceBoard, bool ortho)
        {
            // Remove the friendly king from blockers for See-through attacks from enemy sliding pieces
            ulong blockers = allPieces & ~(1ul << friendlyKingSquare);

            while (pieceBoard != 0)
            {
                Square start = (Square) pieceBoard.PopLSB();
                ulong moveBoard = Magic.GetSliderAttacks(start, blockers, ortho);

                enemySlidingAttackMap |= moveBoard;
            }
        }
    }

    void CalculateAttackData()
    {
        GenerateSlidingAttackMap();

        // int startDirIndex = 0;
        // int endDirIndex = 8;

        // // Skip direction check (Pin / Check)
        // // There are no enemy queens
        // if (board.BitboardSets[enemyColor][PieceHelper.QUEEN] == 0)
        // {
        //     startDirIndex = board.BitboardSets[enemyColor][PieceHelper.ROOK] != 0 ? 0 : 4;
        //     endDirIndex = board.BitboardSets[enemyColor][PieceHelper.BISHOP] != 0 ? 8 : 4;
        // }

        // for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++)
        // {
        //     bool isDiagonal = dirIndex >= 4;

        //     Bitboard slider = isDiagonal ? enemyDiagSliders : enemyOrthoSliders;

        //     // No enemy slider along this direction so skip it
        //     if ((Bits.DirRayMasks[friendlyKingSquare][dirIndex] & slider) == 0)
        //     {
        //         continue;
        //     }

        //     int n = Bits.NumSquaresToEdge[friendlyKingSquare][dirIndex];
        //     int directionOffset = Bits.DirectionOffsets[dirIndex];
        //     bool isFriendlyPieceAlongRay = false;

        //     // For each direction
        //     // Does NOT include the king square
        //     Bitboard rayMask = 0;

        //     for (int i = 1; i <= n; i++)
        //     {
        //         Square sq = (Square) (friendlyKingSquare + directionOffset * i);
        //         rayMask.SetSquare(sq);
        //         Piece pieceAt = board.At(sq);

        //         if (pieceAt != PieceHelper.NONE)
        //         {
        //             if (PieceHelper.IsColor(pieceAt, friendlyColor))
        //             {
        //                 if (!isFriendlyPieceAlongRay)
        //                 {
        //                     // This piece might be pinned
        //                     isFriendlyPieceAlongRay = true;
        //                 }
        //                 else
        //                 {
        //                     // This is the second friendly piece found; it cannot be pinned
        //                     break;
        //                 }
        //             }
        //             // Enemy spotted
        //             else
        //             {
        //                 PieceType pieceType = PieceHelper.GetPieceType(pieceAt);

        //                 if ((isDiagonal && PieceHelper.IsDiagonal(pieceType)) || (!isDiagonal && PieceHelper.IsOrthogonal(pieceType)))
        //                 {
        //                     // Friendly piece blocks the check, so it is pinned
        //                     if (isFriendlyPieceAlongRay)
        //                     {
        //                         // Includes the pinner
        //                         // Does NOT include the king
        //                         pinRays |= rayMask;
        //                     }
        //                     // No friendly piece blocking the check
        //                     // So it is a check
        //                     else
        //                     {
        //                         checkRayBitmask |= rayMask;
        //                         inDoubleCheck = inCheck;
        //                         inCheck = true;
        //                     }

        //                     // Found a check or a pin
        //                     break;
        //                 }
        //                 // The enemy piece is not able to move along this ray
        //                 // So no check or pin
        //                 else
        //                 {
        //                     break;
        //                 }
        //             }
        //         }
        //     }
        //     // If in double check, stop searching for pins / checks
        //     // Since the king moves are the only legal ones
        //     if (inDoubleCheck)
        //     {
        //         break;
        //     }
        // }

        // notPinRays = ~pinRays;

        Bitboard enemyKnightAttacks = 0;
        Bitboard knights = board.BitboardSets[enemyColor][PieceHelper.KNIGHT];

        // For ALL enemy knights
        // Does NOT break
        while (knights != 0)
        {
            Square sq = (Square) knights.PopLSB();
            Bitboard knightAttacks = Bits.KnightMovement[sq];
            enemyKnightAttacks |= knightAttacks;

            // if ((knightAttacks & board.BitboardSets[friendlyColor][PieceHelper.KING]) != 0)
            // {
            //     inDoubleCheck = inCheck;
            //     inCheck = true;
            //     checkRayBitmask.SetSquare(sq);
            // }
        }

        // Pawn attacks
        Bitboard enemyPawnBoard = board.BitboardSets[enemyColor][PieceHelper.PAWN];
        enemyPawnAttackMap = BitboardHelper.PawnAttacks(enemyPawnBoard, enemyColor);

        // if (enemyPawnAttackMap.Contains(friendlyKingSquare))
        // {
        //     inDoubleCheck = inCheck;
        //     inCheck = true;

        //     Bitboard possiblePawnAttackOrigins = Bits.PawnAttacks[friendlyColor][friendlyKingSquare];
        //     Bitboard pawnCheckMap = possiblePawnAttackOrigins & enemyPawnBoard;
        //     checkRayBitmask |= pawnCheckMap;
        // }

        Square enemyKingSquare = board.KingSquares[enemyColor];

        enemyAttackMapNoPawns = enemySlidingAttackMap | enemyKnightAttacks | Bits.KingMovement[enemyKingSquare];
        enemyAttackMap = enemyAttackMapNoPawns | enemyPawnAttackMap;

        // if (!inCheck)
        // {
        //     checkRayBitmask = Bitboard.MaxValue;
        // }
    }
}