using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Printnecdote.Game;

namespace Printnecdote.State
{
    public class PlayerCreationStateMachine : StateMachine
    {
        private Player player;

        public PlayerCreationStateMachine(ulong id) : base (id)
        {
            // Used for 1 time only messages at the start.
            state = -1;
        }

        public override bool UpdateState(ICommandContext context)
        {
            string content = context.Message.Content;
            if(content.ToLower() == "exit")
            {
                return true;
            }

            switch (state)
            {
                case -1:
                    SendMsg("At any point durring character creation, enter `exit` to quit", context);
                    player = new Player(context.User.Id);
                    state = 0;
                    return UpdateState(context);
                case 0:
                    SendMsg("*lore lore lore lore* Whats your name?", context);
                    state = 1;
                    break;
                case 1:
                    player.Name = content;
                    SendMsg($"Are you sure your name is {player.Name}? [yes (y)/no (n)]: ", context);
                    state = 2;
                    break;
                case 2:
                    if(content.ToLower() == "yes" || content.ToLower() == "y")
                    {
                        SendMsg("Press any key to continue.", context);
                        state = 3;
                    }
                    else if (content.ToLower() == "no" || content.ToLower() == "n")
                    {
                        SendMsg("*lore lore lore lore* Whats your name?", context);
                        state = 1;
                    }
                    else
                    {
                        SendMsg($"Are you sure your name is {player.Name}? [yes (y)/no (n)]: ", context);
                    }
                    break;
                case 3:
                    try
                    {
                        AddNewUser();
                        SendMsg($"Welcome to *lore lore lore* {player.Name}", context);
                    }
                    catch (Exception ex)
                    {
                        Program.prog.LogAsync(new LogMessage(LogSeverity.Error, "PlayerCreate", "Something Broke", ex)).GetAwaiter().GetResult();
                        SendMsg("You done broke it. SQL ERROR or PLAYER DICT ERROR.\n" +
                            $"{ex.StackTrace}", context);
                    }
                    return true;
            }

            return false;
        }

        private void AddNewUser()
        {
            Program.game.AddPlayer(player.Id, player);

            // Give player new starting items?
            // Or give info for starter quest?

            // Update database with new player
            using (SqlConnection server = new SqlConnection(Program.conn.ConnectionString))
            {
                server.Open();
                SqlCommand cmd = new SqlCommand($"INSERT INTO {Program.dbo}.PlayerConfig VALUES ('{player.Id}', '{JsonConvert.SerializeObject(player)}')", server);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
