namespace ProtoRPS
{

    /// <summary>
    /// 
    /// Contains the information used to select a specific move
    /// 
    /// </summary>

    class SelectionMethod
    {
        public int Depth { get; }
        public AnalysisMode Mode { get; }


        public SelectionMethod(int length, AnalysisMode mode)
        {
            Depth = length;
            Mode = mode;
        }
    }
}