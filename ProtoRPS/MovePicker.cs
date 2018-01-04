using System;

namespace ProtoRPS
{
    /// <summary>
    /// 
    /// Selects moves to be played in a game of Rock / Paper / Scissors
    /// based on a trainable weighted randomization algorithm
    /// 
    /// </summary>

    class MovePicker
    {

        private const int defaultWeight = 24;
        private const int maxDepth = 3;


        // These weights were chosen in order that the initial frequency
        // with which each of the Selector<MoveType>s is chosen is roughly equal

        private const int threeMoveWeight = 1;
        private const int twoMoveWeight = 3;
        private const int oneMoveWeight = 9;
        private const int zeroMoveWeight = 27;
        private const int modeWeight = 54;
        private const int depthWeight = 54;
        private const int numberOfModes = 5;
        private const int numberOfMoves = 3;


        private static readonly MoveType[] absoluteMoves =
        {
            MoveType.Rock,
            MoveType.Paper,
            MoveType.Scissors
        };

        private static readonly MoveType[] relativeMoves =
        {
            MoveType.Hold,
            MoveType.Ascend,
            MoveType.Descend
        };

        private static readonly ResultType[] validResults = 
        {
            ResultType.Win,
            ResultType.Loss,
            ResultType.Draw
        };

        private static readonly AnalysisMode[] analysisModes = 
        {
            AnalysisMode.PlayerAbsolute,
            AnalysisMode.PlayerRelative,
            AnalysisMode.OpponentAbsolute,
            AnalysisMode.OpponentRelative,
            AnalysisMode.Result
        };


        private bool rewardWin;
        private bool rewardDraw;
        private bool rewardLoss;
        private bool punishWin;
        private bool punishDraw;
        private bool punishLoss;


        private Selector<int> depthSelector;            
        private Selector<AnalysisMode> modeSelector;    

        private Selector<MoveType> zeroMoveTree;
        private Selector<MoveType>[,] oneMoveTree;     
        private Selector<MoveType>[,,] twoMoveTree;
        private Selector<MoveType>[,,,] threeMoveTree;


        private int GetIndex(AnalysisMode mode)
        {
            switch (mode)
            {
                case AnalysisMode.PlayerAbsolute:   return 0;
                case AnalysisMode.PlayerRelative:   return 1;
                case AnalysisMode.OpponentAbsolute: return 2;
                case AnalysisMode.OpponentRelative: return 3;
                case AnalysisMode.Result:           return 4;
                default:
                    throw new ArgumentOutOfRangeException("GetIndex failed to find passed AnalysisMode.");
            }
        }
        

        private int GetIndex(MoveType move)
        {
            switch (move)
            {
                case MoveType.Rock:         return 0;
                case MoveType.Paper:        return 1;
                case MoveType.Scissors:     return 2;
                case MoveType.Hold:         return 0;
                case MoveType.Ascend:       return 1;
                case MoveType.Descend:      return 2;
                default:
                    throw new ArgumentOutOfRangeException("GetIndex failed to find passed MoveType.");
            }
        }


        private int GetIndex(ResultType result)
        {
            switch (result)
            {
                case ResultType.Win:        return 0;
                case ResultType.Loss:       return 1;
                case ResultType.Draw:       return 2;
                default:
                    throw new ArgumentOutOfRangeException("GetIndex failed to find passed ResultType.");
            }
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to increase the frequency of moves that won previously
        /// 
        /// </summary>

        public void RewardWins()
        {
            rewardWin = true;
            punishWin = false;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to decrease the frequency of moves that won previously
        /// 
        /// </summary>

        public void PunishWins()
        {
            rewardWin = false;
            punishWin = true;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to not alter the frequency of moves that won previously
        /// 
        /// </summary>

        public void IgnoreWins()
        {
            rewardWin = false;
            punishWin = false;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to increase the frequency of moves that lost previously
        /// 
        /// </summary>

        public void RewardLosses()
        {
            rewardLoss = true;
            punishLoss = false;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to decrease the frequency of moves that lost previously
        /// 
        /// </summary>

        public void PunishLosses()
        {
            rewardLoss = false;
            punishLoss = true;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to not alter the frequency of moves that lost previously
        /// 
        /// </summary>

        public void IgnoreLosses()
        {
            rewardLoss = false;
            punishLoss = false;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to increase the frequency of moves that drew previously
        /// 
        /// </summary>
        
        public void RewardDraws()
        {
            rewardDraw = true;
            punishDraw = false;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to decrease the frequency of moves that drew previously
        /// 
        /// </summary>

        public void PunishDraws()
        {
            rewardDraw = false;
            punishDraw = true;
        }


        /// <summary>
        /// 
        /// Sets the MovePicker to not alter the frequency of moves that drew previously
        /// 
        /// </summary>

        public void IgnoreDraws()
        {
            rewardDraw = false;
            punishDraw = false;
        }


        /// <summary>
        /// 
        /// Randomly picks a move, and returns a Decision instance that includes the move,
        /// as well as the factors that led to the selection of that move
        /// 
        /// </summary>
        /// <param name="assumedHistory">The history leading up to the move being picked</param>
        /// <returns>The selected move, along with the factors that led to its selection</returns>

        public Decision DecideMove(MoveHistory assumedHistory)
        {
            // Choose a history length
            int depth = depthSelector.Pick();
            if (assumedHistory.Length < depth)
                depth = assumedHistory.Length;


            // Choose a selection mode
            AnalysisMode mode = modeSelector.Pick();
            int firstIndex = GetIndex(mode);


            // Find correct selection tree
            Selector<MoveType> selectionTree = FindSelector(depth, mode, assumedHistory);


            // Build Decision instance
            MoveType pickedMove = selectionTree.Pick();
            MoveHistory pickHistory = new MoveHistory(assumedHistory);
            SelectionMethod pickMethod = new SelectionMethod(depth, mode);


            return new Decision(pickedMove, pickHistory, pickMethod);
        }


        /// <summary>
        /// 
        /// Updates the MovePicker's selection weights based on the results of a previous decision
        /// 
        /// </summary>
        /// <param name="decision"></param>
        /// <param name="result"></param>

        public void Feedback(Decision decision, ResultType result)
        {
            int depth = decision.SelectionMethod.Depth;
            AnalysisMode mode = decision.SelectionMethod.Mode;
            int firstIndex = GetIndex(mode);
            MoveHistory assumedHistory = decision.AssumedHistory;

            Selector<MoveType> selectionTree = FindSelector(depth, mode, assumedHistory);

            switch(result)
            {
                case ResultType.Win:
                    if (rewardWin)
                    {
                        selectionTree.Reward(decision.PickedMove);
                        depthSelector.Reward(depth);
                        if (depth > 1)
                            modeSelector.Reward(mode);
                    }
                    else if (punishWin)
                    {
                        selectionTree.Punish(decision.PickedMove);
                        depthSelector.Punish(depth);
                        if (depth > 1)
                            modeSelector.Punish(mode);
                    }
                    break;

                case ResultType.Loss:
                    if (rewardLoss)
                    {
                        selectionTree.Reward(decision.PickedMove);
                        depthSelector.Reward(depth);
                        if (depth > 1)
                            modeSelector.Reward(mode);
                    }
                    else if (punishLoss)
                    {
                        selectionTree.Punish(decision.PickedMove);
                        depthSelector.Punish(depth);
                        if (depth > 1)
                            modeSelector.Punish(mode);
                    }
                    break;


                case ResultType.Draw:
                    if (rewardDraw)
                    {
                        selectionTree.Reward(decision.PickedMove);
                        depthSelector.Reward(depth);
                        if (depth > 1)
                            modeSelector.Reward(mode);
                    }
                    else if (punishDraw)
                    {
                        selectionTree.Punish(decision.PickedMove);
                        depthSelector.Punish(depth);
                        if (depth > 1)
                            modeSelector.Punish(mode);
                    }
                    break;

                default:
                    break;
            }
        }


        /// <summary>
        /// 
        /// Finds the selector associated with a set of decision factors
        /// 
        /// </summary>
        /// <param name="depth">The number of previous rounds being analyzed</param>
        /// <param name="mode">The type of analysis being used to analyze previous rounds</param>
        /// <param name="assumedHistory">The collection of rounds available to be analyzed</param>
        /// <returns>The Selector associated with the input decision factors</returns>

        Selector<MoveType> FindSelector(int depth, AnalysisMode mode, MoveHistory assumedHistory)
        {
            int firstIndex = GetIndex(mode);

            // Find correct SelectionTree
            Selector<MoveType> selector = null;
            int secondIndex = -1;
            int thirdIndex = -1;
            int fourthIndex = -1;

            if (depth == 0)
                selector = zeroMoveTree;
            else
            {
                switch (mode)
                {
                    case AnalysisMode.PlayerAbsolute:
                        secondIndex = GetIndex(assumedHistory.Rounds[0].PlayerAbsolute);
                        if (depth > 1)
                            thirdIndex = GetIndex(assumedHistory.Rounds[1].PlayerAbsolute);
                        if (depth > 2)
                            fourthIndex = GetIndex(assumedHistory.Rounds[2].PlayerAbsolute);
                        break;

                    case AnalysisMode.PlayerRelative:
                        secondIndex = GetIndex(assumedHistory.Rounds[0].PlayerRelative);
                        if (depth > 1)
                            thirdIndex = GetIndex(assumedHistory.Rounds[1].PlayerRelative);
                        if (depth > 2)
                            fourthIndex = GetIndex(assumedHistory.Rounds[2].PlayerRelative);
                        break;

                    case AnalysisMode.OpponentAbsolute:
                        secondIndex = GetIndex(assumedHistory.Rounds[0].OpponentAbsolute);
                        if (depth > 1)
                            thirdIndex = GetIndex(assumedHistory.Rounds[1].OpponentAbsolute);
                        if (depth > 2)
                            fourthIndex = GetIndex(assumedHistory.Rounds[2].OpponentAbsolute);
                        break;

                    case AnalysisMode.OpponentRelative:
                        secondIndex = GetIndex(assumedHistory.Rounds[0].OpponentRelative);
                        if (depth > 1)
                            thirdIndex = GetIndex(assumedHistory.Rounds[1].OpponentRelative);
                        if (depth > 2)
                            fourthIndex = GetIndex(assumedHistory.Rounds[2].OpponentRelative);
                        break;

                    case AnalysisMode.Result:
                        secondIndex = GetIndex(assumedHistory.Rounds[0].Result);
                        if (depth > 1)
                            thirdIndex = GetIndex(assumedHistory.Rounds[1].Result);
                        if (depth > 2)
                            fourthIndex = GetIndex(assumedHistory.Rounds[2].Result);
                        break;

                    default:
                        throw new InvalidOperationException("FindSelector couldn't resolve AnalysisMode enumeration.");
                }

                if (depth == 1)
                    selector = oneMoveTree[firstIndex, secondIndex];
                else if (depth == 2)
                    selector = twoMoveTree[firstIndex, secondIndex, thirdIndex];
                else if (depth == 3)
                    selector = threeMoveTree[firstIndex, secondIndex, thirdIndex, fourthIndex];
            }

            return selector;
        }


        public MovePicker()
        {
            RewardWins();
            IgnoreDraws();
            PunishLosses();

            depthSelector = new Selector<int>();
            for (int depth = 0; depth < maxDepth; depth++)
                depthSelector.AddCategory(depthWeight * defaultWeight, depth);

            modeSelector = new Selector<AnalysisMode>();
            foreach (AnalysisMode mode in analysisModes)
                modeSelector.AddCategory(modeWeight * defaultWeight, mode);

            zeroMoveTree = new Selector<MoveType>();
            foreach (MoveType move in absoluteMoves)
                zeroMoveTree.AddCategory(zeroMoveWeight * defaultWeight, move);


            // TO DO:  Refactor into a TreeBuilder class
            oneMoveTree = new Selector<MoveType>[numberOfModes, numberOfMoves];
            for (int i = 0; i < numberOfModes; i++)
            {
                // Player Absolute
                for (int j = 0; j < numberOfMoves; j++)
                {
                    oneMoveTree[i, j] = new Selector<MoveType>();
                    foreach (MoveType move in absoluteMoves)
                        oneMoveTree[i, j].AddCategory(oneMoveWeight * defaultWeight, move);
                }

                // Player Relative
                for (int j = 0; j < numberOfMoves; j++)
                {
                    oneMoveTree[i, j] = new Selector<MoveType>();
                    foreach (MoveType move in relativeMoves)
                        oneMoveTree[i, j].AddCategory(oneMoveWeight * defaultWeight, move);
                }

                // Opponent Absolute
                for (int j = 0; j < numberOfMoves; j++)
                {
                    oneMoveTree[i, j] = new Selector<MoveType>();
                    foreach (MoveType move in absoluteMoves)
                        oneMoveTree[i, j].AddCategory(oneMoveWeight * defaultWeight, move);
                }

                // Opponent Relative
                for (int j = 0; j < numberOfMoves; j++)
                {
                    oneMoveTree[i, j] = new Selector<MoveType>();
                    foreach (MoveType move in relativeMoves)
                        oneMoveTree[i, j].AddCategory(oneMoveWeight * defaultWeight, move);
                }

                // Result
                for (int j = 0; j < numberOfMoves; j++)
                {
                    oneMoveTree[i, j] = new Selector<MoveType>();
                    foreach (MoveType move in validResults)
                        oneMoveTree[i, j].AddCategory(oneMoveWeight * defaultWeight, move);
                }
            }

            twoMoveTree = new Selector<MoveType>[numberOfModes, numberOfMoves, numberOfMoves];
            for (int i = 0; i < numberOfModes; i++)
            {
                // Player Absolute
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                    {
                        twoMoveTree[i, j, k] = new Selector<MoveType>();
                        foreach (MoveType move in absoluteMoves)
                            twoMoveTree[i, j, k].AddCategory(twoMoveWeight * defaultWeight, move);
                    }

                // Player Relative
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                    {
                        twoMoveTree[i, j, k] = new Selector<MoveType>();
                        foreach (MoveType move in relativeMoves)
                            twoMoveTree[i, j, k].AddCategory(twoMoveWeight * defaultWeight, move);
                    }

                // Opponent Absolute
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                    {
                        twoMoveTree[i, j, k] = new Selector<MoveType>();
                        foreach (MoveType move in absoluteMoves)
                            twoMoveTree[i, j, k].AddCategory(twoMoveWeight * defaultWeight, move);
                    }

                // Opponent Relative
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                    {
                        twoMoveTree[i, j, k] = new Selector<MoveType>();
                        foreach (MoveType move in relativeMoves)
                            twoMoveTree[i, j, k].AddCategory(twoMoveWeight * defaultWeight, move);
                    }

                // Result
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                    {
                        twoMoveTree[i, j, k] = new Selector<MoveType>();
                        foreach (MoveType move in validResults)
                            twoMoveTree[i, j, k].AddCategory(twoMoveWeight * defaultWeight, move);
                    }
            }

            threeMoveTree = new Selector<MoveType>[numberOfModes, numberOfMoves, numberOfMoves, numberOfMoves];
            for (int i = 0; i < numberOfModes; i++)
            {
                // Player Absolute Moves
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                        for (int l = 0; l < numberOfMoves; l++)
                        {
                            threeMoveTree[i, j, k, l] = new Selector<MoveType>();
                            foreach (MoveType move in absoluteMoves)
                                threeMoveTree[i, j, k, l].AddCategory(threeMoveWeight * defaultWeight, move);
                        }

                // Player Relative Moves
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                        for (int l = 0; l < numberOfMoves; l++)
                        {
                            threeMoveTree[i, j, k, l] = new Selector<MoveType>();
                            foreach (MoveType move in relativeMoves)
                                threeMoveTree[i, j, k, l].AddCategory(threeMoveWeight * defaultWeight, move);
                        }

                // Opponent Absolute Moves
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                        for (int l = 0; l < numberOfMoves; l++)
                        {
                            threeMoveTree[i, j, k, l] = new Selector<MoveType>();
                            foreach (MoveType move in absoluteMoves)
                                threeMoveTree[i, j, k, l].AddCategory(threeMoveWeight * defaultWeight, move);
                        }

                // Opponent Relative Moves
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                        for (int l = 0; l < numberOfMoves; l++)
                        {
                            threeMoveTree[i, j, k, l] = new Selector<MoveType>();
                            foreach (MoveType move in relativeMoves)
                                threeMoveTree[i, j, k, l].AddCategory(threeMoveWeight * defaultWeight, move);
                        }

                // Results
                for (int j = 0; j < numberOfMoves; j++)
                    for (int k = 0; k < numberOfMoves; k++)
                        for (int l = 0; l < numberOfMoves; l++)
                        {
                            threeMoveTree[i, j, k, l] = new Selector<MoveType>();
                            foreach (MoveType move in validResults)
                                threeMoveTree[i, j, k, l].AddCategory(threeMoveWeight * defaultWeight, move);
                        }
            }
        }


        public bool Save()
        {
            return true;
            // TO DO - along with implementing ISerializable
        }


        public bool Load()
        {
            return true;
            // TO DO - along with implementing ISerializable
        }
    }
}
