using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGames.Models.MemoryGame
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public string Group { get; set; }

        public bool isPlaying { get; set; }

        public List<int> Matches { get; set; }

        public Player(string name, string hash)
        {
            this.Name = name;
            this.Hash = hash;
            Matches = new List<int>();
        }
    }
}