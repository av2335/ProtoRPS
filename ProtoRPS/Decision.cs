namespace ProtoRPS
{

    /// <summary>
    /// 
    /// Contains a selected move, along with the move history and selection method that were used
    /// to select that move
    /// 
    /// </summary>

    class Decision
    {
        public MoveType PickedMove { get; }
        public MoveHistory AssumedHistory { get; }
        public SelectionMethod SelectionMethod { get; }


        public Decision(MoveType pickedMove, MoveHistory assumedHistory, SelectionMethod selectionMethod)
        {
            PickedMove = pickedMove;
            AssumedHistory = assumedHistory;
            SelectionMethod = selectionMethod;
        }
    }
}
