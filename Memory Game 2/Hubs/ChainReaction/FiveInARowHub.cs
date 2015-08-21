using System;
using Microsoft.AspNet.SignalR;
using MyGames.ChainReaction;
using MyGames.Models.ChainReaction;

namespace MyGames.Hubs.ChainReaction
{
    public class ChainReactionHub : Hub
    {
        public bool Join(string userName)
        {
            var player = GameState.Instance.GetPlayer(userName);
            if (player != null)
            {
                Player p2;
                var game = GameState.Instance.FindGame(player, out p2);
                if (game != null)
                {
                    if (game.Player1.Name == userName) // Player 1 Refreshed
                    {
                        Clients.All.hello(game.Player1.Name + " Refreshed Page");
                        if (game.WhosTurn == game.Player1.Id) // Player1's turn
                            game.WhosTurn = Context.ConnectionId;
                        game.Player1.Id = Context.ConnectionId;
                    }
                    else if (game.Player2.Name == userName) // Player 2 Refreshed
                    {
                        Clients.All.hello(game.Player2.Name + " Refreshed Page");
                        if (game.WhosTurn == game.Player2.Id) // Player2's turn
                            game.WhosTurn = Context.ConnectionId;
                        game.Player2.Id = Context.ConnectionId;
                    }
                    else
                    {
                        throw new Exception("This player does not exist in this game. This should never happen logically");
                    }

                    player.Id = Context.ConnectionId;
                    Clients.Caller.name = player.Name;

                    Clients.Caller.playerExists(player);
                    Clients.Caller.buildBoard(game);
                    return true;
                }
                else
                {
                    Clients.Caller.waitingList();
                }
            }
            player = GameState.Instance.CreatePlayer(userName);
            player.Id = Context.ConnectionId;

            Clients.Caller.name = player.Name;

            Clients.Caller.playerJoined(player);

            return StartGame(player);
        }
        
        public bool CheckMark(int x, int y)
        {
            var userName = Clients.Caller.name;
            Player p1 = GameState.Instance.GetPlayer(userName), p2;
            if (p1 != null)
            {
                var game = GameState.Instance.FindGame(p1, out p2);
                if (game != null)
                {
                    string id1 = game.Player1.Id;
                    string id2 = game.Player2.Id;

                    if (!string.IsNullOrEmpty(game.WhosTurn) // if WhosTurn is Null then player1
                   && game.WhosTurn != p1.Id)
                        return true;

                    int playerNumber = game.Player1.Id == p1.Id ? 1 : 2;

                    if (game.Board.playerBoard[x, y] == 0)
                    {
                        game.Board.playerBoard[x, y] = playerNumber;
                        game.Board.countBoard[x, y] += 1;
                        if(game.Board.countBoard[x, y] == GetDegree(x, y))
                        {
                            UpdateBoard(game.Board,x,y);
                        }

                        game.WhosTurn = p2.Id;

                        Clients.Client(id1).playNextMove(p1, game.Board);
                        Clients.Client(id2).playNextMove(p1, game.Board);

                        game.lastX = x;
                        game.lastY = y;

                        if (IsGameOver(game.Board.playerBoard))
                        {
                            Clients.Client(id1).winner(p1.Name);
                            Clients.Client(id2).winner(p1.Name);
                            GameState.Instance.ResetGame(game);
                        }
                        return true;
                    }
                    else
                        return true;
                }
            }
            return false;
        }

        private void UpdateBoard(GameBoard board, int x, int y)
        {
            
        }

        private int GetDegree(int x, int y)
        {
            if (
                 (x == 0 && (y == 0 || y == 9)) ||
                 (x == 5 && (y == 0 || y == 9))
            )   return 2;
            else if (
                 (x == 0) || (x == 5) || (y == 0) || (y == 9)
            )   return 3;
            else
                return 4;
        }

        public void TimeOut()
        {
            var userName = Clients.Caller.name;
            Player p1 = GameState.Instance.GetPlayer(userName), p2;
            if (p1 != null)
            {
                var game = GameState.Instance.FindGame(p1, out p2);
                if (game != null)
                {
                    game.WhosTurn = (game.WhosTurn == game.Player1.Id) 
                        ? game.Player2.Id : game.Player1.Id;
                }
            }
        }

        private bool StartGame(Player p1)
        {
            Player p2;
            var game = GameState.Instance.FindGame(p1, out p2);
            if (game != null) // Already player is playing a game
            {
                Clients.Group(p1.Group).buildBoard(game);
                return true;
            }

            p2 = GameState.Instance.GetNewOpponent(p1);
            if (p2 == null) // No Free Players available
            {
                Clients.Caller.waitingList();
                return true;
            }
            game = GameState.Instance.CreateGame(p1, p2);
            game.WhosTurn = p1.Id;
            Clients.Caller.id2 = p2.Id;
            Clients.Group(p1.Group).buildBoard(game);
            return true;
        }
        
        private bool IsGameOver(int [,] board)
        {
            int player = 0;
            bool gameOver = true;

            for(int i = 0; i < 10; i++)
            {
                for(int j = 0; j < 5; j++)
                {
                    if (board[i, j] != player)
                    {
                        if(player == 0)
                            player = board[i, j];
                        else
                        {
                            gameOver = false;
                            break;
                        }

                    }
                }
            }
            return gameOver;
        }
    }
}