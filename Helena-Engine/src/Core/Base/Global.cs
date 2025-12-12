global using System;

// byte: 0~255 unsigned 8 bit integer
// Square only holds 0~63 & 64 for invalid square
global using Square = byte;

// Piece holds both piece type and color information
global using Piece = byte;
// PieceType:
// 0: NONE / 1: PAWN / 2: KNIGHT / 3: BISHOP / 4: ROOK / 5: QUEEN / 6: KING
global using PieceType = byte;

// Color:
// 0: WHITE / 1: BLACK
global using Color = byte;

// Bitboard represents a 64 bit board
global using Bitboard = ulong;

// Just for simplicity
global using MoveList = System.Span<H.Core.Move>;
