namespace ChayaBot.Core.Games
{
    public class Player
    {

        // Properties
        public ulong PlayerId { get; private set; }
        public byte Score { get; private set; }
        public CellTypes Type { get; private set; }


        // Constructor
        public Player(ulong playerId, CellTypes type)
        {
            PlayerId = playerId;
            Type = type;
            Score = 0;
        }


        public void IncrementScore()
        {
            Score++;
        }

    }
}
