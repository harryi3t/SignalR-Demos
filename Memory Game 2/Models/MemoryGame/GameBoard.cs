using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGames.Models.MemoryGame
{
    public class GameBoard
    {
        private List<Card> _pieces = new List<Card>();
        public List<Card> Pieces
        {
            get { return _pieces; }
            set { _pieces = value; }
        }
        public GameBoard()
        {
            int imgIndex = 1;
            for(int i = 0; i < 32; i += 2)
            {
                _pieces.Add(new Card() {
                    Id = i,
                    Pair = i + 1,
                    Name = "card-" + i.ToString(),
                    Image = string.Format("/content/img/{0}.png",imgIndex)
                });
                _pieces.Add(new Card() {
                    Id = i+1,
                    Pair = i,
                    Name = "card-" + (i+1).ToString(),
                    Image = string.Format("/content/img/{0}.png", imgIndex)
                });
                imgIndex++;
            }
            _pieces.shuffle();
        }
    }
}