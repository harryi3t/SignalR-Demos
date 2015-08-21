using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGames.Models.FiveInARow
{
    public class Game
    {
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        public GameBoard Board { get; set; }
        public string WhosTurn { get; set; }
        public int lastX { get; set; }
        public int lastY { get; set; }
    }
}