using ChayaBot.Core.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChayaBot.Services.Games
{
    public class TicTacToeService
    {

        public TicTacToe ticTacToe;



        public void Start(ulong playerOne, ulong playerTwo)
        {
            ticTacToe = new TicTacToe(playerOne, playerTwo);
        }

    }
}
