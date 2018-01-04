namespace ProtoRPS
{
    enum MoveType
    {
        Rock,
        Paper,
        Scissors,
        Hold,       // paper -> paper -> paper -> paper
        Ascend,     // paper -> scissors -> rock -> paper
        Descend     // paper -> rock -> scissors -> paper
    }


    enum ResultType
    {
        Win,
        Loss,
        Draw
    }


    enum AnalysisMode
    {
        PlayerAbsolute,
        PlayerRelative,
        OpponentAbsolute,
        OpponentRelative,
        Result
    }
}