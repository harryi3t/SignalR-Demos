using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MyGames.FiveInARow;
using MyGames.Models.FiveInARow;

namespace MyGames.Hubs.FiveInARow
{
    public class FiveInARowHub : Hub
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
            var connId = Context.ConnectionId;
            Player p1 = GameState.Instance.GetPlayer(userName), p2;
            if (p1 != null)
            {
                var game = GameState.Instance.FindGame(p1, out p2);
                if (game != null)
                {
                    string id1 = game.Player1.Id;
                    string id2 = game.Player2.Id;

                    Clients.All.hello("");
                    Clients.All.hello(connId);
                    Clients.All.hello(id1);
                    Clients.All.hello(id2);

                    if (!string.IsNullOrEmpty(game.WhosTurn) // if WhosTurn is Null then player1
                   && game.WhosTurn != p1.Id)
                        return true;

                    int playerSymbol = game.Player1.Id == p1.Id ? 1 : 2;

                    if (game.Board.board[x, y] == 0)
                    {
                        game.Board.board[x, y] = playerSymbol;
                        game.WhosTurn = p2.Id;
                        //Clients.Group(p1.Group).playNextMove(p1, x, y);

                        Clients.Client(id1).playNextMove(p1, x, y);
                        Clients.Client(id2).playNextMove(p1, x, y);
                        game.lastX = x;
                        game.lastY = y;

                        if (isGameOver(game.Board.board,x,y))
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
        
        private bool isGameOver(int[,] board, int x, int y)
        {
            int length = 0;
            int playerMove = board[x, y];

            //up and down
            int x1 = x - 1, y1 = y;
            while (x1 != -1 && board[x1, y1] == playerMove)
            {
                length++;
                x1--;
            }
            x1 = x + 1;
            while (x1 != 13 && board[x1, y1] == playerMove)
            {
                length++;
                x1++;
            }
            if (length == 4) return true;
            
            //left and right
            x1 = x; y1 = y - 1; length = 0;
            while (y1 != -1 && board[x1, y1] == playerMove)
            {
                length++;
                y1--;
            }
            y1 = y + 1;
            while (y1 != 13 && board[x1, y1] == playerMove)
            {
                length++;
                y1++;
            }
            if (length == 4) return true;

            // primary diagonal
            x1 = x - 1; y1 = y - 1; length = 0;
            while (x1 != -1 && y1 != -1 && board[x1, y1] == playerMove)
            {
                length++;
                x1--; y1--;
            }
            x1 = x + 1; y1 = y + 1;
            while (x1 != 13 && y1 != 13 && board[x1, y1] == playerMove)
            {
                length++;
                x1++; y1++;
            }
            if (length == 4) return true;

            // secondary diagonal
            x1 = x - 1; y1 = y + 1; length = 0;
            while (x1 != -1 && y1 != 13 && board[x1, y1] == playerMove)
            {
                length++;
                x1--; y1++;
            }
            x1 = x + 1; y1 = y - 1;
            while (x1 != 13 && y1 != -1 && board[x1, y1] == playerMove)
            {
                length++;
                x1++; y1--;
            }
            if (length == 4) return true;

            return false;
        }
    }
}