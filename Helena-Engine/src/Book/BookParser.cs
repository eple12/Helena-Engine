namespace H.Book;

using H.Program;
using H.Core;

public static class BookParser
{
    static string SourcePath => """C:\Users\user\Desktop\WorkSpace\Helena-Engine\Helena-Engine\res\games.txt""";
    static string TargetPath => """C:\Users\user\Desktop\WorkSpace\Helena-Engine\Helena-Engine\res\book.txt""";

    static Board board => Main.MainBoard;

    static BookParser()
    {
        
    }

    public static void Parse()
    {
        if (File.Exists(SourcePath) && File.Exists(TargetPath))
        {
            int i = 0;
            foreach (string line in File.ReadLines(SourcePath))
            {
                i++;

                if (!string.IsNullOrEmpty(line))
                {
                    int movesIndex = line.IndexOf("moves");
                    string fen = line.Substring(0, movesIndex - 1);
                    board.LoadPositionFromFEN(fen);

                    File.AppendAllText(TargetPath, $"{board.State.Key}");

                    string[] tokens = line.Split(' ');
                    int tokenMoveIndex = Array.IndexOf(tokens, "moves");
                    for (int idx = 0; idx < tokens.Length - tokenMoveIndex - 1; idx++)
                    {
                        string token = tokens[idx + tokenMoveIndex + 1];

                        if (idx % 2 == 0) // Move string
                        {
                            MoveList moves = Main.MainBoard.MoveGenerator.GenerateMoves();
                            Move m = moves.ToArray().First(a => a.Notation == token);
                            File.AppendAllText(TargetPath, $" {m.MoveValue} ");
                        }
                        else // Weight of this move
                        {
                            File.AppendAllText(TargetPath, token);
                        }
                    }
                    File.AppendAllText(TargetPath, "\n");
                }

                Console.WriteLine(i + ". " + line);
            }
        }
        else
        {
            Console.WriteLine("Files missing!");
        }
    }
}

