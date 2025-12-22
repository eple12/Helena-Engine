namespace H.Engine;

using H.Core;
using System.Collections.Immutable;

public class PvTable {
    // Indexes[ply] => Starting Index
    public static readonly ImmutableArray<int> Indexes = Initialize();

    private static ImmutableArray<int> Initialize()
    {
        var indexes = new int[Constants.MAX_DEPTH + 1];
        int previousPVIndex = 0;
        indexes[0] = previousPVIndex;

        for (int depth = 0; depth < indexes.Length - 1; ++depth)
        {
            indexes[depth + 1] = previousPVIndex + Constants.MAX_DEPTH - depth;
            previousPVIndex = indexes[depth + 1];
        }

        return [.. indexes];
    }

    // Size = 1+2+3+ ... +(N-1)+N = N(N+1)/2
    public const int PvTableSize = Constants.MAX_DEPTH * (Constants.MAX_DEPTH + 1) / 2;

    Move[] Pv = new Move[PvTableSize];

    public Move this[int index] {
        get {
            return Pv[index];
        }
        set {
            Pv[index] = value;
        }
    }

    public void ClearFrom(int index) {
        if (index < PvTableSize)
        {
            Pv[index] = Move.NullMove;
        }
    }

    public void CopyFrom(int target, int source, int length) {
        // A manual copy loop that respects the NullMove terminator,
        // preventing stale data from being copied.
        for (int i = 0; i < length; i++)
        {
            // Bounds check for safety, though it shouldn't be necessary with correct triangular indexing.
            if (target + i >= PvTableSize || source + i >= PvTableSize) break;
            
            Move move = Pv[source + i];
            Pv[target + i] = move;

            if (move == Move.NullMove)
            {
                break;
            }
        }
    }

    public void ClearAll() {
        Array.Clear(Pv);
    }

    public string GetRootString() {
        return string.Join(' ', Pv[..Indexes[1]].TakeWhile(a => a != Move.NullMove).Select(a => a.Notation));
    }

    public void ClearExceptRoot() {
        ClearFrom(Indexes[1]);
    }
}

// Pv Line length at ply

// ply  maxLengthPV
//     +--------------------------------------------+
// 0   |N                                           |
//     +------------------------------------------+-+
// 1   |N-1                                       |
//     +----------------------------------------+-+
// 2   |N-2                                     |
//     +--------------------------------------+-+
// 3   |N-3                                   |
//     +------------------------------------+-+
// 4   |N-4                                 |
// ...                        /
// N-4 |4      |
//     +-----+-+
// N-3 |3    |
//     +---+-+
// N-2 |2  |
//     +-+-+
// N-1 |1|
//     +-+