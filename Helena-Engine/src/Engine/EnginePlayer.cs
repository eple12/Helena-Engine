using System.Diagnostics;
using H.Core;

namespace H.Engine;

public class EnginePlayer
{
    public const int MIN_THINKTIME = Constants.MIN_THINKTIME;

    Engine engine;
    Board board;

    Stopwatch sw = new();

    public EnginePlayer(Board _board)
    {
        engine = new(_board);

        board = _board;
    }

    public void Search(int maxDepth, int searchTimeMS = -1, Action? onSearchComplete = null)
    {
        if (searchTimeMS > 0)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Delay(searchTimeMS, cts.Token)
            .ContinueWith((t) => {
                CancelAndWait();
            });

            onSearchComplete += () => {
                cts.Cancel();
                cts.Dispose();
            };
        }
        
        engine.RequestSearch(maxDepth, onSearchComplete);
    }

    public int DecideThinkTime(int wtime, int btime, int winc, int binc, int max, int min)
    {
        int myTime = board.State.SideToMove ? wtime : btime;
        int myInc = board.State.SideToMove ? winc : binc;

        // Get a fraction of remaining time to use for current move
        double thinkTimeDouble = myTime / 30.0;

        // Clamp think time if a maximum limit is imposed
        thinkTimeDouble = Math.Min(max, thinkTimeDouble);

        // Add increment
        if (myTime > myInc * 2)
        {
            thinkTimeDouble += myInc * 0.6;
        }
        thinkTimeDouble = Math.Ceiling(Math.Max(min, thinkTimeDouble));

        return (int) thinkTimeDouble;
    }

    public void Cancel()
    {
        engine.CancelSearch();
    }

    public void CancelAndWait()
    {
        if (!engine.IsSearching())
        {
            return;
        }

        Cancel();
        while (engine.IsSearching())
        {
            Thread.Sleep(10);
        }
    }

}