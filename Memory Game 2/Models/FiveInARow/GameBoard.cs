using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGames.Models.FiveInARow
{
    public class GameBoard
    {
        public int[,] board { get; set; }
        public GameBoard()
        {
            board = new int[13,13];
        }
    }
}