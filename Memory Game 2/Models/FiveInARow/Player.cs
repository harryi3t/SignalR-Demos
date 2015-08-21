using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGames.Models.FiveInARow
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public string Symbol { get; set; }

        public bool isPlaying { get; set; }
        

        public Player(string name)
        {
            this.Name = name;
        }
    }
}