using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Newtonsoft.Json;
using Printnecdote.Game.Levels;

namespace Printnecdote.Game
{
    /// <summary>
    /// Class to manage and active game. Holds local cahce for game information
    /// </summary>
    class GameController
    {
        public readonly Dictionary<ulong, Player> PlayerDict = new Dictionary<ulong, Player>();

        public GameController()
        {

        }

        /// <summary>
        /// Uses a players id and json data string to add them to the playerDict.
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="jsonString">Json Data String</param>
        /// <returns>Player object that was added</returns>
        public Player InitializePlayer(ulong id, string jsonString)
        {
            if (!PlayerDict.ContainsKey(id))
            {
                PlayerDict.Add(id, JsonConvert.DeserializeObject<Player>(jsonString));
            }
            return PlayerDict[id];
        }

        /// <summary>
        /// Adds a new player to the player dictionary
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="player">Player Object</param>
        /// <returns>True: player was added sucessfuly. False: Player already exsists</returns>
        public bool AddPlayer(ulong id, Player player)
        {
            if (!PlayerDict.ContainsKey(id))
            {
                PlayerDict.Add(id, player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Saves a player to the Database
        /// </summary>
        /// <param name="p">Player to save data for</param>
        /// <returns>True if the save completed</returns>
        public bool SavePlayerData(Player p)
        {
            if (!PlayerDict.ContainsKey(p.Id))
                AddPlayer(p.Id, p); // Player not added.

            using (SqlConnection server = new SqlConnection(Program.conn.ConnectionString))
            {
                try
                {
                    server.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {Program.dbo}.PlayerConfig SET JsonData = '{JsonConvert.SerializeObject(p)}' WHERE UserId = '{p.Id}'", server);
                    if (cmd.ExecuteNonQuery() <= 0)
                    {
                        cmd.CommandText = $"INSERT INTO {Program.dbo}.PlayerConfig VALUES ('{p.Id}', '{JsonConvert.SerializeObject(p)}')";
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Program.prog.LogAsync(new Discord.LogMessage(Discord.LogSeverity.Error, "SavePlayerData", "Player Save Failed!", ex));
                    return false;
                }
            }
        }
    }
}
