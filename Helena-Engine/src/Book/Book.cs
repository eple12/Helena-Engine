namespace H.Book;

using H.Core;

public static class Book
{
    static readonly string BookPath = """C:\Users\user\Desktop\WorkSpace\Helena-Engine\Helena-Engine\res\book.txt""";

    public static Dictionary<ulong, BookPosition> OpeningBook = new();

    static Random RatioRandom = new();

    public static void GenerateTable()
    {
        foreach (string line in File.ReadLines(BookPath))
        {
            List<Move> moves = new List<Move>();
            List<int> nums = new List<int>();
            string[] split = line.Split(' ');
            ulong key = ulong.Parse(split[0]);

            for (int i = 1; i < split.Length; i++)
            {
                if (i % 2 == 1)
                {
                    moves.Add(new Move(ushort.Parse(split[i])));
                }
                else
                {
                    nums.Add(int.Parse(split[i]));
                }
            }

            OpeningBook[key] = new BookPosition(moves, nums);
        }
    }

    public static BookPosition TryGetBookPosition(ulong key)
    {
        if (OpeningBook.ContainsKey(key))
        {
            return OpeningBook[key];
        }
        else
        {
            return new BookPosition();
        }
    }

    public static Move GetRandomMove(ulong key)
    {
        if (OpeningBook.ContainsKey(key))
        {
            BookPosition bookPosition = OpeningBook[key];
            return GetRandomRatio(bookPosition.Moves, bookPosition.Num);
        }
        else
        {
            return Move.NullMove;
        }
    }

    static Move GetRandomRatio(List<Move> options, List<int> ratios)
    {
        if (ratios.Count != options.Count)
        {
            return Move.NullMove;
        }

        int total = ratios.Sum();

        // Random value between 0 and total value
        int randomNumber = RatioRandom.Next(0, total);

        int cumulativeSum = 0;
        for (int i = 0; i < ratios.Count; i++)
        {
            cumulativeSum += ratios[i];
            if (randomNumber < cumulativeSum)
            {
                return options[i];
            }
        }

        // Failsafe
        return options[^1];
    }
}

public struct BookPosition
{
    public List<Move> Moves;
    public List<int> Num;

    public BookPosition()
    {
        Moves = new List<Move>();
        Num = new List<int>();
    }

    public BookPosition(List<Move> moves, List<int> nums)
    {
        Moves = moves;
        Num = nums;
    }

    public bool IsEmpty()
    {
        return Moves.Count < 1;
    }
}