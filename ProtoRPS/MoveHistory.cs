using System.Collections.Generic;

namespace ProtoRPS
{
    /// <summary>
    /// 
    /// Maintains an ordered list of rounds played
    /// 
    /// </summary>

    class MoveHistory
    {
        private const int defaultMaxLength = 3;
        private int maxLength;

        public List<Round> Rounds { get; }
 

        public MoveHistory()
        {
            maxLength = defaultMaxLength;
            Rounds = new List<Round>(maxLength);
        }


        public MoveHistory(int historyLength)
        {
            if (historyLength > 0)
                maxLength = historyLength;
            else
                maxLength = defaultMaxLength;
            Rounds = new List<Round>(maxLength);
        }


        public MoveHistory(MoveHistory source)
        {
            maxLength = source.maxLength;
            Rounds = new List<Round>(source.Rounds);
        }


        public int Length
        {
            get
            {
                if (Rounds == null)
                    return 0;
                return Rounds.Count;
            }
        }


        /// <summary>
        /// 
        /// Adds a round to the history, bumping the oldest round off the end of the
        /// history, if adding the new round would make the list longer than the maximum
        /// 
        /// </summary>
        /// <param name="newRound"></param>

        public void Push(Round newRound)
        {

            if (Rounds.Count == maxLength)
            {
                Rounds.RemoveAt(maxLength - 1);
            }

            Rounds.Insert(0, newRound);
        }
    }
}
