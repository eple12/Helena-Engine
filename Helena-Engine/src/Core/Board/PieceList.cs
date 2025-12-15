namespace H.Core;

// Implemented but NOT used since I really don't want to sync it with MakeMove, UnmakeMove, etc..
// I mean why use it when we can just while-PopLSB

// You can iterate through the occupied squares by PieceList[idx] (0 <= idx < PieceList.Count)
// Make sure the index does not go beyond the Count, since we don't check if the index is valid for performance
// Old data are not cleared for performance
public class PieceList
{
    Square[] squares;
    int[] map; // map[square] => index for squares
    public int Count => count;
    int count = 0;

    public PieceList(int capacity = 64)
    {
        squares = new Square[capacity];
        map = new int[64];
    }

    public void Add(Square square)
    {
        squares[count] = square;
        map[square] = count;
        count++;
    }

    // Remove and Change assume that the square is present for performance
    public void Remove(Square square)
    {
        count--;
        int index = map[square];
        squares[index] = squares[count];
        map[squares[index]] = index;
    }
    public void Change(Square current, Square changeTo)
    {
        map[changeTo] = map[current];
        squares[map[current]] = changeTo;
    }

    public Square this[int index] => squares[index];
}