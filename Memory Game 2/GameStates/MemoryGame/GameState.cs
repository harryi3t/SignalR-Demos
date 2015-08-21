using MyGames.Models.MemoryGame;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MyGames.Hubs.MemoryGame;

namespace MyGames.MemoryGame
{
    public class GameState
    {
        private readonly static Lazy<GameState> _instance =
            new Lazy<GameState>(() => new GameState(
                GlobalHost.ConnectionManager.GetHubContext<GameHub>()
            ));

        private readonly ConcurrentDictionary<string, Player> _players =
            new ConcurrentDictionary<string, Player>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, Game> _games =
            new ConcurrentDictionary<string, Game>(StringComparer.OrdinalIgnoreCase);
        
        private GameState(IHubContext context) {
            Clients = context.Clients;
            Groups = context.Groups;
        }

        public static GameState Instance
        {
            get { return _instance.Value; }
        }
        
        public IGroupManager Groups { get; set; }
        public IHubConnectionContext<dynamic> Clients { get;  set; }

        public Player CreatePlayer(string userName)
        {
            var player = new Player(userName, GetMD5Hash(userName));
            _players[userName] = player;
            return player;
        }

        private string GetMD5Hash(string userName)
        {
            return String.Join("", MD5.Create()
                   .ComputeHash(Encoding.Default.GetBytes(userName))
                   .Select(b=>b.ToString("x2")));
        }

        public Player GetPlayer(string userName)
        {
            return _players.Values.FirstOrDefault(u => u.Name == userName);
        }

        public Player GetPlayerById(string userId)
        {
            return _players.Values.FirstOrDefault(u => u.Id == userId);
        }


        public Player GetNewOpponent(Player player)
        {
            return _players.Values.FirstOrDefault( u => !u.isPlaying 
                && u.Id != player.Id);
        }

        public Player GetOpponent(Game game, Player player)
        {
            return (game.Player1.Id == player.Id) ? game.Player2 : game.Player1;
        }

        public Game CreateGame(Player player1, Player player2)
        {
            Game game = new Game
            {
                Player1 = player1,
                Player2 = player2,
                Board = new GameBoard()
            };

            string group = Guid.NewGuid().ToString("d");
            _games[group] = game;

            player1.isPlaying = true;
            player2.isPlaying = true;
            player1.Group = group;
            player2.Group = group;

            Groups.Add(player1.Id, group);
            Groups.Add(player2.Id, group);

            return game;
        }

        public Game FindGame(Player player, out Player opponent)
        {
            opponent = null;
            if (player.Group == null)
                return null;
            Game game;
            _games.TryGetValue(player.Group, out game);
            if (game != null)
            {
                opponent = (player.Id == game.Player1.Id) ? game.Player2 : game.Player1;
                return game;
            }
            return null;
        }

        public void ResetGame(Game game)
        {
            var groupName = game.Player1.Group;
            var player1Name = game.Player1.Name;
            var player2Name = game.Player2.Name;

            Groups.Remove(game.Player1.Id, groupName);
            Groups.Remove(game.Player2.Id, groupName);

            Player p1, p2;
            _players.TryRemove(player1Name, out p1);
            _players.TryRemove(player1Name, out p2);

            Game g;
            _games.TryRemove(groupName, out g);
        }
    }
}