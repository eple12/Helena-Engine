using System.Diagnostics;
using H.Program;

namespace H.Core;

public static class Perft
{
    static Board board = Main.MainBoard;

    public static PerftPosition[] Perfts = [
        new PerftPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", [20, 400, 8902, 197281, 4865609, 119060324, 3195901860, 84998978956, 2439530234167, 69352859712417, 2097651003696806, 62854969236701747, 1981066775000396239]), // Up to 13 depth since I can't store larger results in 64bit number
        new PerftPosition("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ", [48, 2039, 97862, 4085603, 193690690, 8031647685]),
        new PerftPosition("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1", [14, 191, 2812, 43238, 674624, 11030083, 178633661, 3009794393]),
        new PerftPosition("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1", [6, 264, 9467, 422333, 15833292, 706045033]),
        new PerftPosition("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8", [44, 1486, 62379, 2103487, 89941194]),
        new PerftPosition("r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10", [46, 2079, 89890, 3894594, 164075551, 6923051137, 287188994746, 11923589843526, 490154852788714])
    ];

    public static ulong GoTimedPerft(int depth, bool verbose = false)
    {
        System.Console.WriteLine("Vulk enabled");
        Stopwatch sw = Stopwatch.StartNew();
        ulong r = GoPerft(depth, verbose);
        sw.Stop();
        System.Console.WriteLine(r);
        double inSec = sw.Elapsed.TotalMilliseconds * 0.001;
        System.Console.WriteLine($"Elapsed time: {inSec:F3}s");
        System.Console.WriteLine($"{r/inSec :F3} Nodes/s");

        return r;
    }

    public static ulong GoTimedPerft(ref readonly PerftPosition position, int depth, bool verbose = false)
    {
        board.LoadPositionFromFEN(position.FEN);
        System.Console.WriteLine("Vulk enabled");
        Stopwatch sw = Stopwatch.StartNew();
        ulong r = GoPerft(depth, verbose);
        sw.Stop();
        System.Console.WriteLine($"Result: {r} / {((r == position.Results[depth - 1]) ? "PASS" : "FAIL")}");
        System.Console.WriteLine($"Expected: {position.Results[depth - 1]}");
        double inSec = sw.Elapsed.TotalMilliseconds * 0.001;
        System.Console.WriteLine($"Elapsed time: {inSec:F3}s");
        System.Console.WriteLine($"{r/inSec:F3} Nodes/s");

        return r;
    }

    public static ulong GoPerft(int depth, bool verbose = false)
    {
        return Recursive(depth, 0, verbose);
    }

    public static void GoPosition(ref readonly PerftPosition position, int depth, bool verbose = true)
    {
        board.LoadPositionFromFEN(position.FEN);
        ulong r = GoPerft(depth, verbose);
        board.LoadPositionFromFEN(UCI.STARTPOS_FEN);

        System.Console.WriteLine(r);

        if (position.Results.Length >= depth)
        {
            System.Console.WriteLine((position.Results[depth - 1] == r ? "Pass" : "Fail") + $": Expected {position.Results[depth - 1]}");
        }
    }
    public static void GoPositionAllDepth(ref readonly PerftPosition position)
    {
        int depths = position.Results.Length;
        for (int i = 1; i <= depths; i++)
        {
            GoPosition(in position, i, false);
        }
    }

    public static void GoRoutine()
    {
        UInt128 total = 0;
        Stopwatch sw = Stopwatch.StartNew();

        total += GoTimedPerft(in Perfts[0], 6, false);
        total += GoTimedPerft(in Perfts[1], 5, false);
        total += GoTimedPerft(in Perfts[2], 7, false);
        total += GoTimedPerft(in Perfts[3], 5, false);
        total += GoTimedPerft(in Perfts[4], 5, false);
        total += GoTimedPerft(in Perfts[5], 5, false);

        sw.Stop();
        double inSec = sw.Elapsed.TotalMilliseconds * 0.001;
        System.Console.WriteLine("\nRoutine complete.");
        System.Console.WriteLine($"Elapsed time: {inSec:F3}s");
        System.Console.WriteLine($"{total * 1000 / (UInt128) sw.ElapsedMilliseconds} Nodes/s");
    }

    static ulong Recursive(int depth, int plyFromRoot, bool verbose = false)
    {
        if (depth == 1)
        {
            return (ulong) board.MoveGenerator.GenerateMoves().Length;
        }
        if (depth == 0)
        {
            return 1;
        }

        MoveList moves = board.MoveGenerator.GenerateMoves();
        
        ulong count = 0;
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            ulong r = Recursive(depth - 1, plyFromRoot + 1);
            board.UnmakeMove(move);

            count += r;

            if (plyFromRoot == 0 && verbose)
            {
                System.Console.WriteLine(move.Notation + ": " + r);
            }
        }

        return count;
    }
}

public struct PerftPosition
{
    public readonly string FEN;
    public readonly ulong[] Results;

    public PerftPosition(string fen, ulong[] results)
    {
        FEN = fen;
        Results = results;
    }
}