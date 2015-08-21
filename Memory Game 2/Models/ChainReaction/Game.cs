
namespace MyGames.Models.ChainReaction
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