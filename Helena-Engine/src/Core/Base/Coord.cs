using System.Numerics;

namespace H.Core;

public struct Coord
{
    public readonly int X;
    public readonly int Y;

    public Coord(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Coord(Square square)
    {
        X = SquareHelper.GetFile(square);
        Y = SquareHelper.GetRank(square);
    }

    public static Coord operator +(Coord a, Coord b) => new Coord(a.X + b.X, a.Y + b.Y);
    public static Coord operator -(Coord a, Coord b) => new Coord(a.X - b.X, a.Y - b.Y);
    public static Coord operator *(Coord a, int n) => new Coord(a.X * n, a.Y * n);
    public static Coord operator *(int n, Coord a) => a * n;
    public static bool operator ==(Coord a, Coord b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coord a, Coord b) => a.X != b.X && a.Y != b.Y;

    public bool IsValid => 0 <= X && X <= 7 && 0 <= Y && Y <= 7;
    // Check if this Coord is valid
    public Square GetSquare => IsValid ? SquareHelper.GetSquare(X, Y) : SquareHelper.INVALID_SQUARE;

    public static readonly Coord[] RookDirections = { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1) };
    public static readonly Coord[] BishopDirections = { new Coord(1, 1), new Coord(-1, 1), new Coord(-1, -1), new Coord(1, -1) };

    public override bool Equals(object? obj)
    {
        if (obj is not Coord other) return false;
        return this == other;
    }
    // NOT IMPLEMENTED
    public override int GetHashCode()
    {
        return 0;
    }
}