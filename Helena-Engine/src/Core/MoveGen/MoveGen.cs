namespace H.Core;

// A MoveGen object will be attached to a Board
public class MoveGen
{
    // Board reference
    Board board;

    int MAX_MOVES => Constants.MAX_MOVES;

    // Field used in generation
    int currMoveIdx = 0;

    public MoveGen(Board _board)
    {
        board = _board;
    }

    // QSearch will be implemented in the future
    public MoveList GenerateMoves()
    {
        MoveList moves = new Move[MAX_MOVES];
        
        moves = moves.Slice(0, currMoveIdx);
        return moves;
    }

    void GenerateMoves(ref MoveList moveList)
    {
        
    }

    // Refresh the class every execution
    public void Initialize()
    {
        currMoveIdx = 0;
    }
}