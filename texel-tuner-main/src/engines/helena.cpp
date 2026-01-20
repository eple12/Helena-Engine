#include "helena.h"

#include <array>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include <cmath>
#include <algorithm>
#include <numeric>

using namespace std;
using namespace chess;
using namespace Helena;

// --- Helper Functions ---

// Mirrors Square.cs, Rank/File helpers
namespace SquareHelper {
    int get_rank(Square sq) {
        return sq.rank();
    }
    int get_file(Square sq) {
        return sq.file();
    }
    Square flip_rank(Square sq) {
        return Square(sq.index() ^ 56);
    }
}

// --- Ported Constants and Precomputed Tables from EvaluationConstants.cs ---

namespace EvalConstants {

    // Tunable parameters (initial values)
// // region Original Params
//     const int32_t MaterialValues[] = { S(100, 100), S(300, 300), S(320, 320), S(500, 500), S(900, 900) };
//     const int32_t OutpostBonus = S(25, 20);
//     const int32_t OpenFileBonus = S(20, 5);
//     const int32_t SemiFileBonus = S(15, 5);
//     const int32_t PassedPawnBonus[] = { S(0, 0), S(5, 5), S(10, 10), S(20, 20), S(30, 30), S(40, 40), S(50, 50), S(0, 0) };
//     const int32_t IsolatedPawnPenaltyPerPawn = S(5, 5);
//     const int32_t PawnShelterMissingPenalty = S(15, 0);
//     const int32_t PawnShelterWeakPenalty = S(10, 0);
//     const int32_t KingFileOpenPenalty = S(20, 0);
//     const int32_t KingFileSemiOpenPenalty = S(10, 0);

//     // Mop-up constants (non-tunable, from EvaluationConstants.cs)
//     const int32_t CloserToEnemyKing = S(0, 50);
//     const int32_t EnemyKingCorner = S(0, 50);

//     // PSQTs from Helena-Engine/src/Engine/PSQT.cs
//     const int32_t PSQT_PAWN[64] = {
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0),
//         S( 50, 80), S( 50, 80), S( 50, 80), S( 50, 80), S( 50, 80), S( 50, 80), S( 50, 80), S( 50, 80),
//         S( 10, 50), S( 10, 50), S( 20, 50), S( 30, 50), S( 30, 50), S( 20, 50), S( 10, 50), S( 10, 50),
//         S(  5, 30), S(  5, 30), S( 10, 30), S( 25, 30), S( 25, 30), S( 10, 30), S(  5, 30), S(  5, 30),
//         S(  0, 20), S(  0, 20), S(  0, 20), S( 20, 20), S( 20, 20), S(  0, 20), S(  0, 20), S(  0, 20),
//         S(  5, 10), S( -5, 10), S(-10, 10), S(  0, 10), S(  0, 10), S(-10, 10), S( -5, 10), S(  5, 10),
//         S(  5, 10), S( 10, 10), S( 10, 10), S(-20, 10), S(-20, 10), S( 10, 10), S( 10, 10), S(  5, 10),
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0)
//     };
//     const int32_t PSQT_KNIGHT[64] = {
//         S(-50,-50), S(-40,-40), S(-30,-30), S(-30,-30), S(-30,-30), S(-30,-30), S(-40,-40), S(-50,-50),
//         S(-40,-40), S(-20,-20), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(-20,-20), S(-40,-40),
//         S(-30,-30), S(  0,  0), S( 10, 10), S( 15, 15), S( 15, 15), S( 10, 10), S(  0,  0), S(-30,-30),
//         S(-30,-30), S(  5,  5), S( 15, 15), S( 20, 20), S( 20, 20), S( 15, 15), S(  5,  5), S(-30,-30),
//         S(-30,-30), S(  0,  0), S( 15, 15), S( 20, 20), S( 20, 20), S( 15, 15), S(  0,  0), S(-30,-30),
//         S(-30,-30), S(  5,  5), S( 10, 10), S( 15, 15), S( 15, 15), S( 10, 10), S(  5,  5), S(-30,-30),
//         S(-40,-40), S(-20,-20), S(  0,  0), S(  5,  5), S(  5,  5), S(  0,  0), S(-20,-20), S(-40,-40),
//         S(-50,-50), S(-40,-40), S(-30,-30), S(-30,-30), S(-30,-30), S(-30,-30), S(-40,-40), S(-50,-50)
//     };
//     const int32_t PSQT_BISHOP[64] =  {
//         S(-20,-20), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-20,-20),
//         S(-10,-10), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(-10,-10),
//         S(-10,-10), S(  0,  0), S(  5,  5), S( 10, 10), S( 10, 10), S(  5,  5), S(  0,  0), S(-10,-10),
//         S(-10,-10), S(  5,  5), S(  5,  5), S( 10, 10), S( 10, 10), S(  5,  5), S(  5,  5), S(-10,-10),
//         S(-10,-10), S(  0,  0), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S(  0,  0), S(-10,-10),
//         S(-10,-10), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S(-10,-10),
//         S(-10,-10), S(  5,  5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  5,  5), S(-10,-10),
//         S(-20,-20), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-10,-10), S(-20,-20)
//     };
//     const int32_t PSQT_ROOK[64] =  {
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0),
//         S(  5,  5), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S( 10, 10), S(  5,  5),
//         S( -5, -5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S( -5, -5),
//         S( -5, -5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S( -5, -5),
//         S( -5, -5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S( -5, -5),
//         S( -5, -5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S( -5, -5),
//         S( -5, -5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S( -5, -5),
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  5,  5), S(  5,  5), S(  0,  0), S(  0,  0), S(  0,  0)
//     };
//     const int32_t PSQT_QUEEN[64] =  {
//         S(-20,-20), S(-10,-10), S(-10,-10), S( -5, -5), S( -5, -5), S(-10,-10), S(-10,-10), S(-20,-20),
//         S(-10,-10), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(-10,-10),
//         S(-10,-10), S(  0,  0), S(  5,  5), S(  5,  5), S(  5,  5), S(  5,  5), S(  0,  0), S(-10,-10),
//         S( -5, -5), S(  0,  0), S(  5,  5), S(  5,  5), S(  5,  5), S(  5,  5), S(  0,  0), S( -5, -5),
//         S(  0,  0), S(  0,  0), S(  5,  5), S(  5,  5), S(  5,  5), S(  5,  5), S(  0,  0), S( -5, -5),
//         S(-10,-10), S(  5,  5), S(  5,  5), S(  5,  5), S(  5,  5), S(  5,  5), S(  0,  0), S(-10,-10),
//         S(-10,-10), S(  0,  0), S(  5,  5), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(-10,-10),
//         S(-20,-20), S(-10,-10), S(-10,-10), S( -5, -5), S( -5, -5), S(-10,-10), S(-10,-10), S(-20,-20)
//     };
//     const int32_t PSQT_KING[64] = {
//         S(-80, -20), S(-70, -10), S(-70, -10), S(-70, -10), S(-70, -10), S(-70, -10), S(-70, -10), S(-80, -20), 
//         S(-60,  -5), S(-60,   0), S(-60,   5), S(-60,   5), S(-60,   5), S(-60,   5), S(-60,   0), S(-60,  -5), 
//         S(-40, -10), S(-50,  -5), S(-50,  20), S(-60,  30), S(-60,  30), S(-50,  20), S(-50,  -5), S(-40, -10), 
//         S(-30, -15), S(-40, -10), S(-40,  35), S(-50,  45), S(-50,  45), S(-40,  35), S(-40, -10), S(-30, -15), 
//         S(-20, -20), S(-30, -15), S(-30,  30), S(-40,  40), S(-40,  40), S(-30,  30), S(-30, -15), S(-20, -20), 
//         S(-10, -25), S(-20, -20), S(-20,  20), S(-20,  25), S(-20,  25), S(-20,  20), S(-20, -20), S(-10, -25), 
//         S( 20, -30), S( 20, -25), S( -5,   0), S( -5,   0), S( -5,   0), S( -5,   0), S( 20, -25), S( 20, -30), 
//         S( 20, -50), S( 30, -30), S( 10, -30), S(  0, -30), S(  0, -30), S( 10, -30), S( 30, -30), S( 20, -50)
//     };
// // endregion
// // region Latest Tuning txt
//     const int32_t MaterialValues[] = { S(101, 149), S(472, 557), S(441, 516), S(522, 904), S(1433, 1819) };
//     const int32_t OutpostBonus = S(23, 29);
//     const int32_t OpenFileBonus = S(39, 7);
//     const int32_t SemiFileBonus = S(16, 6);
//     const int32_t PassedPawnBonus[] = { S(0, 0), S(-12, 14), S(-19, 22), S(-24, 64), S(19, 101), S(80, 194), S(99, 256), S(0, 0) };
//     const int32_t IsolatedPawnPenaltyPerPawn = S(20, 20);
//     const int32_t PawnShelterMissingPenalty = S(15, 0);
//     const int32_t PawnShelterWeakPenalty = S(10, 0);
//     const int32_t KingFileOpenPenalty = S(20, 0);
//     const int32_t KingFileSemiOpenPenalty = S(10, 0);

//     // Mop-up constants (non-tunable, from EvaluationConstants.cs)
//     const int32_t CloserToEnemyKing = S(0, 50);
//     const int32_t EnemyKingCorner = S(0, 50);

//     // PSQTs from Helena-Engine/src/Engine/PSQT.cs
//     const int32_t PSQT_PAWN[64] = {
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0),
//         S(52, 84), S(60, 64), S(74, 94), S(45, 64), S(72, 78), S(24, 56), S(74, 97), S(29, 37),
//         S(30, 54), S(20, 35), S(35, 50), S(31, 32), S(27, 44), S(27, 27), S(16, 37), S(19, 38),
//         S(10, 27), S(21, 29), S(3, 34), S(46, 18), S(14, 27), S(27, 4), S(-5, 33), S(25, 4),
//         S(7, 24), S(12, 12), S(4, 33), S(26, 12), S(21, 10), S(8, 7), S(1, -1), S(9, 12),
//         S(8, 2), S(9, 19), S(8, 14), S(2, 12), S(4, 10), S(-4, -9), S(3, 7), S(7, -8),
//         S(8, 16), S(15, 8), S(15, 21), S(-10, 14), S(-11, 6), S(11, 1), S(15, 1), S(7, 10),
//         S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0)
//     };
//     const int32_t PSQT_KNIGHT[64] = {
//         S(96, -10), S(-110, -61), S(28, -8), S(-47, -26), S(-1, -12), S(-22, -34), S(-38, -36), S(-40, -66),
//         S(-16, -26), S(-25, -36), S(22, -2), S(-26, -24), S(24, 17), S(-21, -43), S(0, -16), S(-30, -55),
//         S(-15, -30), S(1, -18), S(7, 4), S(22, -8), S(21, 2), S(19, -2), S(5, -3), S(-14, -31),
//         S(-37, -26), S(26, -7), S(4, 12), S(37, -6), S(10, 23), S(35, -11), S(12, 9), S(-18, -38),
//         S(-26, -17), S(6, -8), S(16, 5), S(28, 7), S(21, -1), S(24, 7), S(3, -8), S(-16, -21),
//         S(-12, -26), S(7, 7), S(14, 10), S(21, -4), S(23, 12), S(12, -8), S(8, 11), S(-25, -32),
//         S(-35, -29), S(-10, -16), S(9, -4), S(6, -4), S(10, -4), S(2, 0), S(-7, -15), S(-34, -26),
//         S(-44, -40), S(-33, -26), S(-21, -19), S(-18, -23), S(-18, -17), S(-17, -23), S(-29, -20), S(-39, -36)
//     };
//     const int32_t PSQT_BISHOP[64] =  {
//         S(38, 2), S(-27, -6), S(19, 8), S(-2, -14), S(-8, -6), S(0, -26), S(14, 4), S(-25, -36),
//         S(12, -12), S(-26, -24), S(24, 17), S(-21, -43), S(20, 4), S(10, -15), S(15, 0), S(-9, -28),
//         S(-13, -16), S(7, -23), S(11, -8), S(19, -2), S(15, 7), S(21, 4), S(-7, 4), S(11, -22),
//         S(-21, -13), S(22, -21), S(-5, 8), S(30, -16), S(17, 14), S(17, -3), S(9, 18), S(-4, -18),
//         S(-9, -20), S(8, -13), S(11, -11), S(19, 2), S(13, 2), S(24, 19), S(18, 4), S(-8, -8),
//         S(-6, -10), S(16, -9), S(18, 7), S(12, -8), S(13, 16), S(15, 8), S(15, 21), S(0, -6),
//         S(-1, -14), S(6, -4), S(5, -9), S(2, 0), S(13, 5), S(6, 14), S(11, 15), S(-3, 4),
//         S(-11, -9), S(2, -3), S(2, 3), S(3, -3), S(1, 10), S(1, 4), S(-11, 6), S(-16, -8)
//     };
//     const int32_t PSQT_ROOK[64] =  {
//         S(29, 18), S(8, -4), S(2, 4), S(10, -16), S(24, 14), S(-5, -16), S(22, -2), S(-26, -24),
//         S(39, 32), S(-1, -23), S(40, 24), S(30, 5), S(35, 20), S(21, 2), S(17, 14), S(22, -8),
//         S(1, -18), S(9, -12), S(5, -3), S(16, -1), S(-7, 4), S(21, -12), S(-11, -3), S(12, -31),
//         S(-15, -2), S(20, -26), S(7, 4), S(12, -8), S(4, 13), S(6, -8), S(1, -10), S(3, -18),
//         S(-4, -26), S(9, -8), S(3, -8), S(14, 9), S(18, 4), S(2, 2), S(4, 0), S(1, -24),
//         S(3, -8), S(2, -18), S(3, 6), S(5, -2), S(5, 11), S(10, 4), S(9, -4), S(-4, -14),
//         S(-10, -24), S(2, 0), S(13, 5), S(6, 14), S(6, 10), S(7, 14), S(9, 11), S(-3, -8),
//         S(-8, -7), S(-7, -13), S(11, 20), S(26, 29), S(14, 31), S(4, 12), S(-11, -6), S(-9, 2)
//     };
//     const int32_t PSQT_QUEEN[64] =  {
//         S(-18, -16), S(0, -26), S(14, 4), S(-10, -21), S(17, -7), S(-36, -34), S(14, 7), S(-41, -63),
//         S(10, -6), S(10, -15), S(15, 0), S(1, -18), S(-3, -6), S(7, -23), S(6, -13), S(-1, -22),
//         S(-5, -13), S(16, -1), S(-2, 9), S(26, -7), S(-6, 2), S(22, -21), S(-10, 3), S(10, -36),
//         S(2, -1), S(12, -8), S(9, 18), S(11, -3), S(6, -5), S(13, -8), S(1, -21), S(4, -13),
//         S(3, -8), S(14, 9), S(23, 9), S(7, 7), S(9, 5), S(11, -14), S(8, -3), S(-3, -23),
//         S(-7, -4), S(10, 3), S(10, 16), S(15, 9), S(14, 1), S(6, -4), S(5, -9), S(-8, -10),
//         S(3, -5), S(6, 14), S(11, 15), S(7, 14), S(9, 11), S(12, 7), S(12, 13), S(3, -3),
//         S(-9, 0), S(1, 4), S(-11, 6), S(-1, 7), S(4, 9), S(1, 12), S(7, 3), S(-15, 14)
//     };
//     const int32_t PSQT_KING[64] = {
//         S(-56, -6), S(-75, -26), S(-48, -12), S(-96, -34), S(-46, 7), S(-91, -53), S(-50, -6), S(-70, -35),
//         S(-45, -5), S(-59, -18), S(-63, -1), S(-53, -18), S(-54, -8), S(-51, -7), S(-55, -3), S(-44, -6),
//         S(-47, -6), S(-29, -17), S(-61, 17), S(-43, 4), S(-70, 33), S(-30, -6), S(-43, -1), S(-28, -18),
//         S(-26, -2), S(-34, -18), S(-39, 25), S(-42, 32), S(-49, 24), S(-31, 27), S(-37, -18), S(-16, -6),
//         S(-2, -16), S(-28, -13), S(-26, 30), S(-34, 21), S(-32, 37), S(-28, 12), S(-27, -9), S(-15, -22),
//         S(-5, -14), S(-10, -16), S(-11, 16), S(-19, 16), S(-15, 16), S(-18, 20), S(-7, -15), S(-4, -11),
//         S(16, -20), S(17, -11), S(4, 11), S(7, 7), S(7, 13), S(8, 7), S(21, -5), S(21, -16),
//         S(9, -34), S(19, -18), S(19, -16), S(6, -8), S(17, -17), S(0, 4), S(14, -16), S(-9, 16)
//     };
// // endregion
// region Modified
    const int32_t MaterialValues[] = { S(100, 100), S(300, 300), S(320, 320), S(500, 500), S(900, 900) };
    const int32_t BishopPairBonus = S(15, 15);
    const int32_t OutpostBonus = S(23, 29);
    const int32_t OpenFileBonus = S(39, 7);
    const int32_t SemiFileBonus = S(16, 6);
    const int32_t PassedPawnBonus[] = { S(0, 0), S(10, 14), S(15, 22), S(25, 64), S(35, 101), S(80, 194), S(99, 256), S(0, 0) };
    const int32_t PassedPawnProtectedBonus = S(50, 70);
    const int32_t PassedPawnBlockedPenalty[] = { S(15, 15), S(10, 10), S(5, 5), S(3, 3), S(1, 1) }; // Knight, Bishop, Rook, Queen, King
    const int32_t DoubledPawnPenalty = S(10, 10);
    const int32_t IsolatedPawnPenaltyByCount[] = { S(0, 0), S(20, 30), S(30, 40), S(40, 50), S(50, 60), S(60, 70), S(70, 80), S(80, 90), S(90, 100) };
    const int32_t PawnShelterMissingPenalty = S(15, 0);
    const int32_t PawnShelterWeakPenalty = S(10, 0);
    const int32_t KingFileOpenPenalty = S(20, 0);
    const int32_t KingFileSemiOpenPenalty = S(10, 0);
    const int32_t PawnStormPenaltyByDistance[] = { S(30, 25), S(25, 15), S(20, 5), S(15, 0) }; // [0-3]
    
    // Mobility bonuses
    const int32_t KnightMobilityBonus[] = { S(0, 0), S(20, 20), S(30, 30), S(35, 35), S(35, 35), S(35, 35), S(35, 35), S(35, 35), S(35, 35) }; // [0-8]
    const int32_t BishopMobilityBonus[] = { S(0, 0), S(20, 20), S(30, 30), S(35, 35), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40) }; // Max 13
    const int32_t RookMobilityBonus[] = { S(0, 0), S(20, 20), S(30, 30), S(35, 35), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40) }; // Max 14
    const int32_t QueenMobilityBonus[] = { S(0, 0), S(20, 20), S(30, 30), S(35, 35), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40), S(40, 40) }; // Max 27

    // Mop-up constants (non-tunable, from EvaluationConstants.cs)
    const int32_t CloserToEnemyKing = S(0, 50);
    const int32_t EnemyKingCorner = S(0, 50);

    // PSQTs from Helena-Engine/src/Engine/PSQT.cs
    const int32_t PSQT_PAWN[64] = {
        S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0),
        S(52, 84), S(60, 64), S(74, 94), S(45, 64), S(72, 78), S(24, 56), S(74, 97), S(29, 37),
        S(30, 54), S(20, 35), S(35, 50), S(31, 32), S(27, 44), S(27, 27), S(16, 37), S(19, 38),
        S(10, 27), S(21, 29), S(3, 34), S(46, 18), S(14, 27), S(27, 4), S(-5, 33), S(25, 4),
        S(7, 24), S(12, 12), S(4, 33), S(26, 12), S(21, 10), S(8, 7), S(1, -1), S(9, 12),
        S(8, 2), S(9, 19), S(8, 14), S(2, 12), S(4, 10), S(-4, -9), S(3, 7), S(7, -8),
        S(8, 16), S(15, 8), S(15, 21), S(-10, 14), S(-11, 6), S(11, 1), S(15, 1), S(7, 10),
        S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0), S(  0,  0)
    };
    const int32_t PSQT_KNIGHT[64] = {
        S(-40, -10), S(-110, -61), S(28, -8), S(-47, -26), S(-1, -12), S(-22, -34), S(-38, -36), S(-40, -66),
        S(-16, -26), S(-25, -36), S(22, -2), S(-26, -24), S(24, 17), S(-21, -43), S(0, -16), S(-30, -55),
        S(-15, -30), S(1, -18), S(7, 4), S(22, -8), S(21, 2), S(19, -2), S(5, -3), S(-14, -31),
        S(-37, -26), S(26, -7), S(4, 12), S(37, -6), S(10, 23), S(35, -11), S(12, 9), S(-18, -38),
        S(-26, -17), S(6, -8), S(16, 5), S(28, 7), S(21, -1), S(24, 7), S(3, -8), S(-16, -21),
        S(-12, -26), S(7, 7), S(14, 10), S(21, -4), S(23, 12), S(12, -8), S(8, 11), S(-25, -32),
        S(-35, -29), S(-10, -16), S(9, -4), S(6, -4), S(10, -4), S(2, 0), S(-7, -15), S(-34, -26),
        S(-44, -40), S(-33, -26), S(-21, -19), S(-18, -23), S(-18, -17), S(-17, -23), S(-29, -20), S(-39, -36)
    };
    const int32_t PSQT_BISHOP[64] =  {
        S(-30, 2), S(-27, -6), S(19, 8), S(-2, -14), S(-8, -6), S(0, -26), S(14, 4), S(-25, -36),
        S(-10, -12), S(-26, -24), S(24, 17), S(-21, -43), S(20, 4), S(10, -15), S(15, 0), S(-9, -28),
        S(-13, -16), S(7, -23), S(11, -8), S(19, -2), S(15, 7), S(21, 4), S(-7, 4), S(11, -22),
        S(-21, -13), S(22, -21), S(-5, 8), S(30, -16), S(17, 14), S(17, -3), S(9, 18), S(-4, -18),
        S(-9, -20), S(8, -13), S(11, -11), S(19, 2), S(13, 2), S(24, 19), S(18, 4), S(-8, -8),
        S(-6, -10), S(16, -9), S(18, 7), S(12, -8), S(13, 16), S(15, 8), S(15, 21), S(0, -6),
        S(-1, -14), S(6, -4), S(5, -9), S(2, 0), S(13, 5), S(6, 14), S(11, 15), S(-3, 4),
        S(-11, -9), S(2, -3), S(2, 3), S(3, -3), S(1, 10), S(1, 4), S(-11, 6), S(-16, -8)
    };
    const int32_t PSQT_ROOK[64] =  {
        S(29, 18), S(8, -4), S(2, 4), S(10, -16), S(24, 14), S(-5, -16), S(22, -2), S(-26, -24),
        S(39, 32), S(-1, -23), S(40, 24), S(30, 5), S(35, 20), S(21, 2), S(17, 14), S(22, -8),
        S(1, -18), S(9, -12), S(5, -3), S(16, -1), S(-7, 4), S(21, -12), S(-11, -3), S(12, -31),
        S(-15, -2), S(20, -26), S(7, 4), S(12, -8), S(4, 13), S(6, -8), S(1, -10), S(3, -18),
        S(-4, -26), S(9, -8), S(3, -8), S(14, 9), S(18, 4), S(2, 2), S(4, 0), S(1, -24),
        S(3, -8), S(2, -18), S(3, 6), S(5, -2), S(5, 11), S(10, 4), S(9, -4), S(-4, -14),
        S(-10, -24), S(2, 0), S(13, 5), S(6, 14), S(6, 10), S(7, 14), S(9, 11), S(-3, -8),
        S(-8, -7), S(-7, -13), S(11, 20), S(26, 29), S(14, 31), S(4, 12), S(-11, -6), S(-9, 2)
    };
    const int32_t PSQT_QUEEN[64] =  {
        S(-18, -16), S(0, -26), S(14, 4), S(-10, -21), S(17, -7), S(-36, -34), S(14, 7), S(-41, -63),
        S(10, -6), S(10, -15), S(15, 0), S(1, -18), S(-3, -6), S(7, -23), S(6, -13), S(-1, -22),
        S(-5, -13), S(16, -1), S(-2, 9), S(26, -7), S(-6, 2), S(22, -21), S(-10, 3), S(10, -36),
        S(2, -1), S(12, -8), S(9, 18), S(11, -3), S(6, -5), S(13, -8), S(1, -21), S(4, -13),
        S(3, -8), S(14, 9), S(23, 9), S(7, 7), S(9, 5), S(11, -14), S(8, -3), S(-3, -23),
        S(-7, -4), S(10, 3), S(10, 16), S(15, 9), S(14, 1), S(6, -4), S(5, -9), S(-8, -10),
        S(3, -5), S(6, 14), S(11, 15), S(7, 14), S(9, 11), S(12, 7), S(12, 13), S(3, -3),
        S(-9, 0), S(1, 4), S(-11, 6), S(-1, 7), S(4, 9), S(1, 12), S(7, 3), S(-15, 14)
    };
    const int32_t PSQT_KING[64] = {
        S(-56, -6), S(-75, -26), S(-48, -12), S(-96, -34), S(-46, 7), S(-91, -53), S(-50, -6), S(-70, -35),
        S(-45, -5), S(-59, -18), S(-63, -1), S(-53, -18), S(-54, -8), S(-51, -7), S(-55, -3), S(-44, -6),
        S(-47, -6), S(-29, -17), S(-61, 17), S(-43, 4), S(-70, 33), S(-30, -6), S(-43, -1), S(-28, -18),
        S(-26, -2), S(-34, -18), S(-39, 25), S(-42, 32), S(-49, 24), S(-31, 27), S(-37, -18), S(-16, -6),
        S(-2, -16), S(-28, -13), S(-26, 30), S(-34, 21), S(-32, 37), S(-28, 12), S(-27, -9), S(-15, -22),
        S(-5, -14), S(-10, -16), S(-11, 16), S(-19, 16), S(-15, 16), S(-18, 20), S(-7, -15), S(-4, -11),
        S(16, -20), S(17, -11), S(4, 11), S(7, 7), S(7, 13), S(8, 7), S(21, -5), S(21, -16),
        S(9, -34), S(19, -18), S(19, -16), S(6, -8), S(17, -17), S(0, 4), S(14, -16), S(-9, 16)
    };
// endregion

    const int32_t* PsqtTables[] = {
        PSQT_PAWN, PSQT_KNIGHT, PSQT_BISHOP, PSQT_ROOK, PSQT_QUEEN, PSQT_KING
    };

    // Precomputed data
    int DistanceFromSquare[64][64];
    int DistanceFromCenter[64];
    Bitboard PawnForwardMask[2][8];
    Bitboard PassedPawnMask[2][64];
    Bitboard ForwardPawnAttackers[2][64];
    Bitboard KingArea[64];
    Bitboard AdjacentFiles[8];
    Bitboard TripleFiles[8];


    void initialize() {
        // DistanceFromSquare and DistanceFromCenter
        for (int i = 0; i < 64; ++i) {
            for (int j = 0; j < 64; ++j) {
                DistanceFromSquare[i][j] = abs(SquareHelper::get_rank(Square(i)) - SquareHelper::get_rank(Square(j))) + abs(SquareHelper::get_file(Square(i)) - SquareHelper::get_file(Square(j)));
            }
            int rank = SquareHelper::get_rank(Square(i));
            int file = SquareHelper::get_file(Square(i));
            int dist_d4 = abs(rank - 3) + abs(file - 3);
            int dist_e4 = abs(rank - 3) + abs(file - 4);
            int dist_d5 = abs(rank - 4) + abs(file - 3);
            int dist_e5 = abs(rank - 4) + abs(file - 4);
            DistanceFromCenter[i] = min({dist_d4, dist_e4, dist_d5, dist_e5});
        }
        
        for(int i = 0; i < 8; ++i) {
            AdjacentFiles[i] = 0;
            if (i > 0) AdjacentFiles[i] |= Bitboard(File(i - 1));
            if (i < 7) AdjacentFiles[i] |= Bitboard(File(i + 1));

            TripleFiles[i] = Bitboard(File(i));
            if (i > 0) TripleFiles[i] |= Bitboard(File(i - 1));
            if (i < 7) TripleFiles[i] |= Bitboard(File(i + 1));
        }

        // Pawn masks
        for (int rank = 0; rank < 8; rank++) {
             PawnForwardMask[0][rank] = ~Bitboard(0) << (8 * (rank + 1));
             PawnForwardMask[1][rank] = (~Bitboard(0) >> (8 * (8 - rank)));
        }

        for (int color = 0; color < 2; ++color) {
            for (int sq_idx = 0; sq_idx < 64; ++sq_idx) {
                Square sq(sq_idx);
                int rank = SquareHelper::get_rank(sq);
                int file = SquareHelper::get_file(sq);

                PassedPawnMask[color][sq_idx] = PawnForwardMask[color][rank] & TripleFiles[file];
                ForwardPawnAttackers[color][sq_idx] = PassedPawnMask[color][sq_idx] & AdjacentFiles[file];
            }
        }
        
        // King area
        for (int i = 0; i < 64; ++i) {
            KingArea[i] = attacks::king(Square(i)) | Bitboard::fromSquare(Square(i));
        }
    }
} // namespace EvalConstants

// --- Trace struct to count feature occurrences ---

struct Trace
{
    // [color][feature]
    int material[2][5]{};
    int psqt[2][6][64]{};
    int bishop_pair[2]{};
    int mobility_knight[2][9]{}; // [move_count] - count of knights with each mobility
    int mobility_bishop[2][14]{}; // [move_count] - count of bishops with each mobility
    int mobility_rook[2][15]{}; // [move_count] - count of rooks with each mobility
    int mobility_queen[2][28]{}; // [move_count] - count of queens with each mobility
    int outpost_bonus[2]{};
    int open_file_bonus[2]{};
    int semi_open_file_bonus[2]{};
    int passed_pawn_bonus[2][8]{};
    int passed_pawn_protected[2]{};
    int passed_pawn_blocked[2][5]{}; // Knight, Bishop, Rook, Queen, King
    int doubled_pawn[2]{};
    int isolated_pawn_penalty[2][9]{}; // By count [0-8]
    int pawn_shelter_missing[2]{};
    int pawn_shelter_weak[2]{};
    int king_file_open[2]{};
    int king_file_semi_open[2]{};
    int pawn_storm[2][4]{}; // By distance [0-3]
};


// --- Evaluation Logic Ported from C# ---

static bool is_supported_by_pawn(const Board& board, Color color, Square square) {
    return bool(attacks::pawn(~color, square) & board.pieces(PieceType::PAWN, color));
}

static void trace_outpost(const Board& board, Trace& trace, Color color) {
    auto knights = board.pieces(PieceType::KNIGHT, color);
    auto bishops = board.pieces(PieceType::BISHOP, color);
    const auto enemy_pawns = board.pieces(PieceType::PAWN, ~color);
    
    int outpost_count = 0;

    while (knights) {
        Square sq = knights.pop();
        int sq_idx = sq.index();
        if (!(EvalConstants::ForwardPawnAttackers[color][sq_idx] & enemy_pawns)) {
            if (is_supported_by_pawn(board, color, sq)) {
                outpost_count++;
            }
        }
    }
    while (bishops) {
        Square sq = bishops.pop();
        int sq_idx = sq.index();
        if (!(EvalConstants::ForwardPawnAttackers[color][sq_idx] & enemy_pawns)) {
            if (is_supported_by_pawn(board, color, sq)) {
                outpost_count++;
            }
        }
    }
    
    trace.outpost_bonus[color] = outpost_count;
}

static void trace_open_file(const Board& board, Trace& trace, Color color) {
    const auto friendly_pawns = board.pieces(PieceType::PAWN, color);
    const auto enemy_pawns = board.pieces(PieceType::PAWN, ~color);
    auto rooks = board.pieces(PieceType::ROOK, color);

    int open_file_count = 0;
    int semi_open_file_count = 0;

    while (rooks) {
        Square sq = rooks.pop();
        auto file_bb = Bitboard(sq.file());

        if (!(file_bb & (friendly_pawns | enemy_pawns))) {
            open_file_count++;
        } else if (!(file_bb & friendly_pawns)) {
            semi_open_file_count++;
        }
    }
    
    trace.open_file_bonus[color] = open_file_count;
    trace.semi_open_file_bonus[color] = semi_open_file_count;
}

static void trace_pawn_value(const Board& board, Trace& trace, Color color) {
    const auto friendly_pawns = board.pieces(PieceType::PAWN, color);
    const auto enemy_pawns = board.pieces(PieceType::PAWN, ~color);

    int num_isolated = 0;
    
    auto friendly_pawns_copy = friendly_pawns;
    while (friendly_pawns_copy) {
        Square sq = friendly_pawns_copy.pop();
        int rank = (color == Color::WHITE) ? (int)sq.rank() : 7 - (int)sq.rank();
        int sq_idx = sq.index();
        int file = sq.file();
        Bitboard file_bb = Bitboard(File(file));
        
        // Passed pawn check
        if (!(EvalConstants::PassedPawnMask[color][sq_idx] & enemy_pawns)) {
            trace.passed_pawn_bonus[color][rank]++;
            
            // Protected passed pawn
            if (attacks::pawn(~color, sq) & friendly_pawns) {
                trace.passed_pawn_protected[color]++;
            }
            
            // Blocked passed pawn penalty
            if (!(EvalConstants::PassedPawnMask[color][sq_idx] & file_bb & friendly_pawns)) {
                int next_sq_idx = sq_idx + (color == Color::WHITE ? 8 : -8);
                if (next_sq_idx >= 0 && next_sq_idx < 64) {
                    Square next_sq(next_sq_idx);
                    Piece piece = board.at(next_sq);
                    if (piece != Piece::NONE) {
                        PieceType pt = piece.type();
                        // Cannot be a pawn since it is a passed pawn
                        if (pt != PieceType::PAWN) {
                            // Knight=1 -> 0, Bishop=2 -> 1, Rook=3 -> 2, Queen=4 -> 3, King=5 -> 4
                            int pt_idx = static_cast<int>(pt) - static_cast<int>(PieceType::KNIGHT);
                            if (pt_idx >= 0 && pt_idx < 5) {
                                trace.passed_pawn_blocked[color][pt_idx]++;
                            }
                        }
                    }
                }
            }
        }

        // Isolated pawn check
        if (!(EvalConstants::AdjacentFiles[file] & friendly_pawns)) {
            num_isolated++;
        }
        
        // Doubled pawn check - C# checks each pawn individually
        // If a file has more than one pawn, each pawn gets penalized
        if ((file_bb & friendly_pawns).count() > 1) {
            trace.doubled_pawn[color]++;
        }
    }
    
    // Isolated pawn penalty by count
    if (num_isolated < 9) {
        trace.isolated_pawn_penalty[color][num_isolated]++;
    } else {
        trace.isolated_pawn_penalty[color][8]++; // Cap at 8
    }
}

static void trace_king_safety(const Board& board, Trace& trace, Color color) {
    const auto friendly_pawns = board.pieces(PieceType::PAWN, color);
    const auto opp_pawns = board.pieces(PieceType::PAWN, ~color);
    Square king_sq = board.kingSq(color);
    int king_file = king_sq.file();
    int shelter_base_rank = (color == Color::WHITE) ? 1 : 6;

    // Reset counters
    int pawn_shelter_missing = 0;
    int pawn_shelter_weak = 0;
    int king_file_open = 0;
    int king_file_semi_open = 0;

    int file_start = max(0, king_file - 1);
    int file_end = min(7, king_file + 1);
    for (int file_idx = file_start; file_idx <= file_end; ++file_idx) {
        Bitboard file_bb = Bitboard(File(file_idx));
        auto shelter_pawns = friendly_pawns & file_bb;

        if (!shelter_pawns) {
            pawn_shelter_missing++;
            if (!(opp_pawns & file_bb)) { // Fully open
                king_file_open++;
            } else { // Semi-open
                king_file_semi_open++;
            }
        } else {
            Square pawn_sq = (color == Color::WHITE) ? shelter_pawns.lsb() : shelter_pawns.msb();
            int rank_dist = abs((int)pawn_sq.rank() - shelter_base_rank);
            if (rank_dist > 1) {
                pawn_shelter_weak += (rank_dist - 1);
            }
        }
        
        // Pawn storm penalty
        Bitboard enemy_pawn_this_file = opp_pawns & file_bb;
        if (enemy_pawn_this_file) {
            Square leading_pawn_sq = (color == Color::WHITE) ? enemy_pawn_this_file.lsb() : enemy_pawn_this_file.msb();
            int leading_rank_dist = abs((int)leading_pawn_sq.rank() - shelter_base_rank);
            if (leading_rank_dist <= 3) {
                trace.pawn_storm[color][leading_rank_dist]++;
            }
        }
    }
    
    trace.pawn_shelter_missing[color] = pawn_shelter_missing;
    trace.pawn_shelter_weak[color] = pawn_shelter_weak;
    trace.king_file_open[color] = king_file_open;
    trace.king_file_semi_open[color] = king_file_semi_open;
}

// Calculate MopUp evaluation (non-tunable)
// Uses initial material values to approximate materialDelta
static tune_t calculate_mopup(const Board& board, Color side_to_move) {
    // Calculate material delta using initial parameter values (midgame values for approximation)
    int32_t material_delta = 0;
    for (int pt_idx = 0; pt_idx < 5; ++pt_idx) {
        PieceType pt = PieceType(static_cast<PieceType::underlying>(pt_idx));
        int white_count = board.pieces(pt, Color::WHITE).count();
        int black_count = board.pieces(pt, Color::BLACK).count();
        // Use midgame value (first element of S macro)
        int32_t mg_value = mg_score(EvalConstants::MaterialValues[pt_idx]);
        material_delta += (white_count - black_count) * mg_value;
    }
    
    // MopUp only applies when winning by at least a pawn (using endgame value)
    const int32_t pawn_endgame = eg_score(EvalConstants::MaterialValues[0]);
    if (material_delta < pawn_endgame) {
        return 0.0;
    }
    
    Color friendly_color = side_to_move;
    Color enemy_color = ~side_to_move;
    Square friendly_king = board.kingSq(friendly_color);
    Square enemy_king = board.kingSq(enemy_color);
    
    int32_t distance = EvalConstants::DistanceFromSquare[friendly_king.index()][enemy_king.index()];
    int32_t enemy_king_dist_from_center = EvalConstants::DistanceFromCenter[enemy_king.index()];
    
    // Use endgame values for MopUp (it only applies in endgame)
    int32_t closer_bonus = (14 - distance) * eg_score(EvalConstants::CloserToEnemyKing);
    int32_t corner_bonus = enemy_king_dist_from_center * eg_score(EvalConstants::EnemyKingCorner);
    
    tune_t mopup_score = closer_bonus + corner_bonus;
    
    // Adjust for side to move perspective
    return (friendly_color == Color::WHITE) ? mopup_score : -mopup_score;
}

static void trace_bishop_pair(const Board& board, Trace& trace, Color color) {
    int bishop_count = board.pieces(PieceType::BISHOP, color).count();
    if (bishop_count >= 2) {
        trace.bishop_pair[color] = 1;
    }
}

static void trace_piece_mobility(const Board& board, Trace& trace, Color color) {
    const auto enemy_pawns = board.pieces(PieceType::PAWN, ~color);
    Bitboard enemy_pawn_attacks = 0;
    auto enemy_pawns_copy = enemy_pawns;
    while (enemy_pawns_copy) {
        Square sq = enemy_pawns_copy.pop();
        enemy_pawn_attacks |= attacks::pawn(~color, sq);
    }
    const auto friendly_occupancy = board.us(color);
    Bitboard safe_squares = ~enemy_pawn_attacks & ~friendly_occupancy;
    const auto all_occupancy = board.occ();
    
    // Knight mobility - count how many knights have each mobility count
    auto knights = board.pieces(PieceType::KNIGHT, color);
    while (knights) {
        Square sq = knights.pop();
        Bitboard moves = attacks::knight(sq) & safe_squares;
        int move_count = min(moves.count(), 8);
        trace.mobility_knight[color][move_count]++;
    }
    
    // Bishop mobility - count how many bishops have each mobility count
    auto bishops = board.pieces(PieceType::BISHOP, color);
    while (bishops) {
        Square sq = bishops.pop();
        Bitboard moves = attacks::bishop(sq, all_occupancy) & safe_squares;
        int move_count = min(moves.count(), 13);
        trace.mobility_bishop[color][move_count]++;
    }
    
    // Rook mobility - count how many rooks have each mobility count
    auto rooks = board.pieces(PieceType::ROOK, color);
    while (rooks) {
        Square sq = rooks.pop();
        Bitboard moves = attacks::rook(sq, all_occupancy) & safe_squares;
        int move_count = min(moves.count(), 14);
        trace.mobility_rook[color][move_count]++;
    }
    
    // Queen mobility - count how many queens have each mobility count
    auto queens = board.pieces(PieceType::QUEEN, color);
    while (queens) {
        Square sq = queens.pop();
        Bitboard moves = (attacks::bishop(sq, all_occupancy) | attacks::rook(sq, all_occupancy)) & safe_squares;
        int move_count = min(moves.count(), 27);
        trace.mobility_queen[color][move_count]++;
    }
}

// Main trace function
static tune_t trace_evaluate(const Board& board, Trace& trace) {
    // --- Trace Material and PSQT (tunable features) ---
    // Optimize: cache piece sets to avoid repeated lookups
    for (int c = 0; c < 2; ++c) {
        Color color = Color(c);
        for (int pt_idx = 0; pt_idx < 6; ++pt_idx) {
            PieceType pt = PieceType(static_cast<PieceType::underlying>(pt_idx));
            auto bb = board.pieces(pt, color);
            int count = bb.count();
            
            if (pt_idx < 5) { // Material only for P,N,B,R,Q
                 trace.material[c][pt_idx] = count;
            }
            
            // Trace PSQT
            while(bb) {
                Square sq = bb.pop();
                int sq_idx = (color == Color::WHITE) ? (sq.index() ^ 56) : sq.index();
                trace.psqt[c][pt_idx][sq_idx]++;
            }
        }
    }
    
    // --- Trace other tunable features ---
    trace_bishop_pair(board, trace, Color::WHITE);
    trace_bishop_pair(board, trace, Color::BLACK);
    trace_piece_mobility(board, trace, Color::WHITE);
    trace_piece_mobility(board, trace, Color::BLACK);
    trace_outpost(board, trace, Color::WHITE);
    trace_outpost(board, trace, Color::BLACK);
    trace_open_file(board, trace, Color::WHITE);
    trace_open_file(board, trace, Color::BLACK);
    trace_pawn_value(board, trace, Color::WHITE);
    trace_pawn_value(board, trace, Color::BLACK);
    trace_king_safety(board, trace, Color::WHITE);
    trace_king_safety(board, trace, Color::BLACK);

    // Calculate MopUp as non-tunable score
    return calculate_mopup(board, board.sideToMove());
}


// --- Tuner Interface Implementation ---

parameters_t HelenaEval::get_initial_parameters() {
    EvalConstants::initialize();
    parameters_t parameters;
    
    // Mop-up parameters are removed as they are not used.
    get_initial_parameter_array(parameters, EvalConstants::MaterialValues, 5);
    for(int i = 0; i < 6; ++i) {
        get_initial_parameter_array(parameters, EvalConstants::PsqtTables[i], 64);
    }
    get_initial_parameter_single(parameters, EvalConstants::BishopPairBonus);
    get_initial_parameter_array(parameters, EvalConstants::KnightMobilityBonus, 9);
    get_initial_parameter_array(parameters, EvalConstants::BishopMobilityBonus, 14);
    get_initial_parameter_array(parameters, EvalConstants::RookMobilityBonus, 15);
    get_initial_parameter_array(parameters, EvalConstants::QueenMobilityBonus, 28);
    get_initial_parameter_single(parameters, EvalConstants::OutpostBonus);
    get_initial_parameter_single(parameters, EvalConstants::OpenFileBonus);
    get_initial_parameter_single(parameters, EvalConstants::SemiFileBonus);
    get_initial_parameter_array(parameters, EvalConstants::PassedPawnBonus, 8);
    get_initial_parameter_single(parameters, EvalConstants::PassedPawnProtectedBonus);
    get_initial_parameter_array(parameters, EvalConstants::PassedPawnBlockedPenalty, 5);
    get_initial_parameter_single(parameters, EvalConstants::DoubledPawnPenalty);
    get_initial_parameter_array(parameters, EvalConstants::IsolatedPawnPenaltyByCount, 9);
    get_initial_parameter_single(parameters, EvalConstants::PawnShelterMissingPenalty);
    get_initial_parameter_single(parameters, EvalConstants::PawnShelterWeakPenalty);
    get_initial_parameter_single(parameters, EvalConstants::KingFileOpenPenalty);
    get_initial_parameter_single(parameters, EvalConstants::KingFileSemiOpenPenalty);
    get_initial_parameter_array(parameters, EvalConstants::PawnStormPenaltyByDistance, 4);
    
    return parameters;
}

coefficients_t get_coefficients(const Trace& trace) {
    coefficients_t coeffs;
    
    // The order must match get_initial_parameters
    // Mop-up coefficients are removed
    for(int i = 0; i < 5; ++i) coeffs.push_back(trace.material[0][i] - trace.material[1][i]);
    for(int pt=0; pt<6; ++pt) {
        for (int sq=0; sq<64; ++sq) {
            coeffs.push_back(trace.psqt[0][pt][sq] - trace.psqt[1][pt][sq]);
        }
    }
    coeffs.push_back(trace.bishop_pair[0] - trace.bishop_pair[1]);
    for(int i = 0; i < 9; ++i) coeffs.push_back(trace.mobility_knight[0][i] - trace.mobility_knight[1][i]);
    for(int i = 0; i < 14; ++i) coeffs.push_back(trace.mobility_bishop[0][i] - trace.mobility_bishop[1][i]);
    for(int i = 0; i < 15; ++i) coeffs.push_back(trace.mobility_rook[0][i] - trace.mobility_rook[1][i]);
    for(int i = 0; i < 28; ++i) coeffs.push_back(trace.mobility_queen[0][i] - trace.mobility_queen[1][i]);
    coeffs.push_back(trace.outpost_bonus[0] - trace.outpost_bonus[1]);
    coeffs.push_back(trace.open_file_bonus[0] - trace.open_file_bonus[1]);
    coeffs.push_back(trace.semi_open_file_bonus[0] - trace.semi_open_file_bonus[1]);
    for(int i = 0; i < 8; ++i) coeffs.push_back(trace.passed_pawn_bonus[0][i] - trace.passed_pawn_bonus[1][i]);
    coeffs.push_back(trace.passed_pawn_protected[0] - trace.passed_pawn_protected[1]);
    for(int i = 0; i < 5; ++i) coeffs.push_back(-(trace.passed_pawn_blocked[0][i] - trace.passed_pawn_blocked[1][i]));
    coeffs.push_back(-(trace.doubled_pawn[0] - trace.doubled_pawn[1]));
    for(int i = 0; i < 9; ++i) coeffs.push_back(-(trace.isolated_pawn_penalty[0][i] - trace.isolated_pawn_penalty[1][i]));
    coeffs.push_back(-(trace.pawn_shelter_missing[0] - trace.pawn_shelter_missing[1]));
    coeffs.push_back(-(trace.pawn_shelter_weak[0] - trace.pawn_shelter_weak[1]));
    coeffs.push_back(-(trace.king_file_open[0] - trace.king_file_open[1]));
    coeffs.push_back(-(trace.king_file_semi_open[0] - trace.king_file_semi_open[1]));
    for(int i = 0; i < 4; ++i) coeffs.push_back(-(trace.pawn_storm[0][i] - trace.pawn_storm[1][i]));
    
    return coeffs;
}


EvalResult HelenaEval::get_fen_eval_result(const std::string& fen) {
    Board board(fen);
    return get_external_eval_result(board);
}

EvalResult HelenaEval::get_external_eval_result(const chess::Board& board) {
    Trace trace{};
    tune_t non_tunable_score = trace_evaluate(board, trace);

    EvalResult result;
    auto coeffs = get_coefficients(trace);

    // The tuner expects coefficients from the perspective of the side to move.
    if (board.sideToMove() == Color::BLACK) {
        for(auto& c : coeffs) {
            c = -c;
        }
        non_tunable_score = -non_tunable_score;
    }

    result.coefficients = coeffs;
    result.score = non_tunable_score;
    result.endgame_scale = 1.0; 

    return result;
}


void print_single(stringstream& ss, const parameters_t& parameters, int& index, const string& name) {
    const auto p = parameters[index++];
    ss << "    public static readonly S " << name << " = new S(" << round(p[0]) << ", " << round(p[1]) << ");" << endl;
}

void print_array(stringstream& ss, const parameters_t& parameters, int& index, const string& name, int count) {
    ss << "    public static readonly S[] " << name << " = {";
    for (int i = 0; i < count; ++i) {
        const auto p = parameters[index++];
        ss << "new S(" << round(p[0]) << ", " << round(p[1]) << ")" << (i == count - 1 ? "" : ", ");
    }
    ss << "};" << endl;
}

void print_psqt(stringstream& ss, const parameters_t& parameters, int& index, const string& name) {
     ss << "    public static readonly S[] " << name << " = {";
    for (int i = 0; i < 64; ++i) {
        if (i % 8 == 0) ss << endl << "        ";
        const auto p = parameters[index++];
        ss << "new S(" << round(p[0]) << ", " << round(p[1]) << ")" << (i == 63 ? "" : ", ");
    }
    ss << endl << "    };" << endl;
}


void HelenaEval::print_parameters(const parameters_t& parameters) {
    int index = 0;
    stringstream ss;
    ss << "// Tuned values for Helena-Engine" << endl;
    ss << "namespace H.Engine;" << endl << endl;
    ss << "public static partial class TunedEvaluation" << endl;
    ss << "{" << endl;

    print_array(ss, parameters, index, "MaterialValues", 5);
    
    const char* psqt_names[] = {"PawnPsqt", "KnightPsqt", "BishopPsqt", "RookPsqt", "QueenPsqt", "KingPsqt"};
    for(int i=0; i<6; ++i) {
        print_psqt(ss, parameters, index, psqt_names[i]);
    }
    
    ss << endl;
    ss << "    // Piece Features" << endl;
    print_single(ss, parameters, index, "BishopPairBonus");
    print_array(ss, parameters, index, "KnightMobilityBonus", 9);
    print_array(ss, parameters, index, "BishopMobilityBonus", 14);
    print_array(ss, parameters, index, "RookMobilityBonus", 15);
    print_array(ss, parameters, index, "QueenMobilityBonus", 28);
    print_single(ss, parameters, index, "OutpostBonus");
    print_single(ss, parameters, index, "OpenFileBonus");
    print_single(ss, parameters, index, "SemiFileBonus");
    
    ss << endl;
    ss << "    // Pawn Features" << endl;
    print_array(ss, parameters, index, "PassedPawnBonus", 8);
    print_single(ss, parameters, index, "PassedPawnProtectedBonus");
    print_array(ss, parameters, index, "PassedPawnBlockedPenalty", 5);
    print_single(ss, parameters, index, "DoubledPawnPenalty");
    print_array(ss, parameters, index, "IsolatedPawnPenaltyByCount", 9);
    
    ss << endl;
    ss << "    // King Safety" << endl;
    print_single(ss, parameters, index, "PawnShelterMissingPenalty");
    print_single(ss, parameters, index, "PawnShelterWeakPenalty");
    print_single(ss, parameters, index, "KingFileOpenPenalty");
    print_single(ss, parameters, index, "KingFileSemiOpenPenalty");
    print_array(ss, parameters, index, "PawnStormPenaltyByDistance", 4);

    ss << "}" << endl;

    cout << ss.str();
}
