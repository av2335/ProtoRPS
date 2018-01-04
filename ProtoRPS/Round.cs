namespace ProtoRPS
{

    /// <summary>
    /// 
    /// Contains the moves chosen by both players for a specific round, as well as the
    /// result of that round
    /// 
    /// </summary>

    class Round
    {
        public MoveType PlayerAbsolute { get; }
        public MoveType PlayerRelative { get; }
        public MoveType OpponentAbsolute { get; }
        public MoveType OpponentRelative { get; }
        public ResultType Result { get; }


        public Round
        (
            MoveType playerAbsolute, 
            MoveType playerRelative, 
            MoveType opponentAbsolute, 
            MoveType opponentRelative, 
            ResultType result
        )
        {
            PlayerAbsolute = playerAbsolute;
            PlayerRelative = playerRelative;
            OpponentAbsolute = opponentAbsolute;
            OpponentRelative = opponentRelative;
            Result = result;
        }
    }
}
