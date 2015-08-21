using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using MyGames.Models.MemoryGame;
using MyGames.MemoryGame;

namespace MyGames.Hubs.MemoryGame
{
    /*
    Server Methods:
        Join
        Flip
        Checkcard
    Client Variables:
           name, hash, id
    Client Methods:
        playerExists
        playerJoined
        waitingList
        buildBoard
        flipCard
        resetFlip
        showMatch
        winner
    */
    public class GameHub : Hub
    {
        public bool Join(string userName)
        {
            var player = GameState.Instance.GetPlayer(userName);
            if (player != null)
            {
                Player p2;
                var game = GameState.Instance.FindGame(player, out p2);
                Clients.Caller.buildBoard(game);
                Clients.Caller.playerExists(player);

                Clients.Caller.name = player.Name;
                Clients.Caller.hash = player.Hash;
                Clients.Caller.id = player.Id;

                return true;
            }
            player = GameState.Instance.CreatePlayer(userName);
            player.Id = Context.ConnectionId;

            Clients.Caller.name = player.Name;
            Clients.Caller.hash = player.Hash;
            Clients.Caller.id = player.Id;
            
            Clients.Caller.playerJoined(player);

            return StartGame(player);
        }

        public bool Flip(string cardName)
        {
            var userName = Clients.Caller.name;
            Player p1 = GameState.Instance.GetPlayer(userName), p2;
            if(p1 != null)
            {
                var game = GameState.Instance.FindGame(p1, out p2);
                if(game != null)
                {
                    if (!string.IsNullOrEmpty(game.WhosTurn)
                    && game.WhosTurn != p1.Id)
                        return true;

                    Card card = FindCard(game, cardName);
                    Clients.Group(p1.Group).flipCard(card);
                    return true;
                }
            }
            
            return false;
        }
        
        public bool CheckCard(string cardName)
        {
            var userName = Clients.Caller.name;
            Player p1 = GameState.Instance.GetPlayer(userName), p2;
            if(p1 != null)
            {
                var game = GameState.Instance.FindGame(p1, out p2);
                if(game != null)
                {
                    if (!string.IsNullOrEmpty(game.WhosTurn)
                   && game.WhosTurn != p1.Id)
                        return true;
                    
                    Card card = FindCard(game, cardName);

                    if (game.LastCard == null) // First Flipped Card
                    {
                        game.WhosTurn = p1.Id;
                        game.LastCard = card;
                        return true;
                    }

                    // Second Flip 
                    bool isMatch = IsMatch(game, card);
                    if (isMatch)
                    {
                        StoreMatch(p1, card);
                        game.LastCard = null;
                        Clients.Group(p1.Group).showMatch(card, userName);

                        // Check winner
                        if (p1.Matches.Count >= 16)
                        {
                            Clients.Group(p1.Group).winner(card, userName);
                            GameState.Instance.ResetGame(game);
                            return true;
                        }
                        return true;
                    }
                    p2 = GameState.Instance.GetOpponent(game, p1);
                    // shift to other player
                    game.WhosTurn = p2.Id;

                    Clients.Group(p2.Group).resetFlip(game.LastCard, card);
                    game.LastCard = null;
                    return true;
                }
            }
           return false;
        }

        private void StoreMatch(Player p1, Card card)
        {
            p1.Matches.Add(card.Id);
            p1.Matches.Add(card.Pair);
        }

        private bool IsMatch(Game game, Card card)
        {
            if (card == null)
                return false;
            if(game.LastCard != null)
            {
                if (game.LastCard.Pair == card.Id)
                    return true;
            }
            return false;
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
            Clients.Group(p1.Group).buildBoard(game);
            return true;
        }

        private Card FindCard(Game game, string cardName)
        {
            return game.Board.Pieces.FirstOrDefault(c => c.Name == cardName);
        }
    }
}