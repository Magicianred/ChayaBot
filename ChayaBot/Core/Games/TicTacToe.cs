using System;
using System.Collections.Generic;
using System.Text;

namespace ChayaBot.Core.Games
{
    public class TicTacToe
    {

        // Properties
        public Player PlayerOne { get; private set; }
        public Player PlayerTwo { get; private set; }
        public DateTime StartTime { get; private set; }
        public byte Rounds { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public byte Size { get; private set; }


        // Fields
        private TicTacToeGrid grid;
        private Random random;


        // Contrustor
        public TicTacToe(ulong playerOneId, ulong playerTwoId, byte size = 3)
        {
            PlayerOne = new Player(playerOneId, CellTypes.CROSS);
            PlayerTwo = new Player(playerTwoId, CellTypes.CIRCLE);
            StartTime = DateTime.Now;
            Size = size;
            Rounds = 0;

            grid = new TicTacToeGrid(size);
            random = new Random();

            NewRound();
        }


        private void SetCurrentPlayer()
        {
            // Randomly sets the current player if its the start of a round
            if (CurrentPlayer == null)
            {
                CurrentPlayer = random.Next(0, 2) == 0 ? PlayerOne : PlayerTwo;
            }
            else
            {
                CurrentPlayer = CurrentPlayer == PlayerOne ? PlayerTwo : PlayerOne;
            }
        }

        private bool IsWon(int startX, int startY, CellTypes type)
        {
            return (grid.IsWon(startX, startY, type, Directions.HORIZONTAL) ||
                grid.IsWon(startX, startY, type, Directions.VERTICAL) ||
                grid.IsWon(startX, startY, type, Directions.DIAGONAL));
        }

        public void NewRound()
        {
            grid.InitializeCells();
            Rounds++;
            SetCurrentPlayer();
        }

        public CellChangeTypeResult ChangeCellType(int x, int y)
        {
            // Validate position
            if (!grid.TryGetCell(x, y, out CellTypes? cellType))
                return CellChangeTypeResult.INVALID_POSITION;

            // Check if the cell is already filled
            if (cellType != CellTypes.NONE)
                return CellChangeTypeResult.ALREADY_FILLED;

            // Change the cell's type
            grid.ChangeCellType(x, y, CurrentPlayer.Type);

            // Check if the player has won
            if (IsWon(x, y, CurrentPlayer.Type))
            {
                CurrentPlayer.IncrementScore();
                return CellChangeTypeResult.WON;
            }

            // Check if the grid is filled
            if (grid.IsFilled())
                return CellChangeTypeResult.DRAW;

            SetCurrentPlayer();
            return CellChangeTypeResult.CHANGED;
        }

        public CellTypes[,] GetGrid()
        {
            return grid.GetCopy();
        }

    }
}
