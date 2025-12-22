namespace H.Engine;

using H.Core;
using System;

public class TT
{
    public const int LookupFailed = int.MinValue;

    public const byte Exact = 0;
    // Lower bound
    public const byte Alpha = 1;
    // Upper bound
    public const byte Beta = 2;

    TTEntry[] entries;

    public readonly ulong Size;

    Board board;

    public TT(Board _board, ulong sizeMB = Constants.TT_SIZE_MB)
    {
        Size = sizeMB * 1024 * 1024 / (ulong) TTEntry.GetSize();
        entries = new TTEntry[Size];

        board = _board;
    }

    public void Clear()
    {
        entries = new TTEntry[Size];
    }

    public ulong Index => board.State.Key % Size;

    public Move GetStoredMove()
    {
        return entries[Index].key == board.State.Key ? entries[Index].move : Move.NullMove;
    }

    public int LookupEval(int depth, int plyFromRoot, int alpha, int beta)
    {
        ref TTEntry entry = ref entries[Index];

        if (entry.depth >= depth && entry.key == board.State.Key)
        {
            int correctEval = CorrectRetrievedMateScore(entry.value, plyFromRoot);

            if (entry.nodeType == Exact)
            {
                return correctEval;
            }

            if (entry.nodeType == Alpha && correctEval <= alpha)
            {
                return correctEval;
            }
            if (entry.nodeType == Beta && correctEval >= beta)
            {
                return correctEval;
            }
        }

        return LookupFailed;
    }

    public void StoreEval(int depth, int ply, int eval, byte type, Move move)
    {
        ref var e = ref entries[Index];

        bool shouldReplace = 
            e.key == 0 ||
            depth >= e.depth || 
            type == Exact;

        if (!shouldReplace)
        {
            return;
        }

        TTEntry te = new TTEntry(board.State.Key, CorrectMateScoreForStorage(eval, ply), move, (byte) depth, type);
        entries[Index] = te;
    }



    // Store mate eval from node
    int CorrectMateScoreForStorage(int eval, int ply)
    {
        if (Evaluation.IsMateEval(eval))
        {
            int sign = Math.Sign(eval);
            return (eval * sign + ply) * sign;
        }
        return eval;
    }

    // Mate from root
    int CorrectRetrievedMateScore(int eval, int ply)
    {
        if (Evaluation.IsMateEval(eval))
        {
            int sign = Math.Sign(eval);
            return (eval * sign - ply) * sign;
        }
        return eval;
    }
}

// 16 bytes per entry
public struct TTEntry
{
    public readonly ulong key;
    public readonly int value;
    public readonly Move move;
    public readonly byte depth;
    public readonly byte nodeType;

    public TTEntry(ulong key, int value, Move move, byte depth, byte nodeType)
    {
        this.key = key;
        this.value = value;
        this.move = move;
        this.depth = depth;
        this.nodeType = nodeType;
    }

    public static int GetSize()
    {
        return System.Runtime.InteropServices.Marshal.SizeOf<TTEntry> ();
    }
}