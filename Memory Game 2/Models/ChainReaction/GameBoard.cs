
namespace MyGames.Models.ChainReaction
{
    public class GameBoard
    {
        public int[,] countBoard { get; set; }
        public int[,] playerBoard { get; set; }
        public GameBoard()
        {
            countBoard = new int[10,6];
            playerBoard = new int[10, 6];
        }
    }
}