using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Printnecdote.Modules
{
    public class UtilityCommandsModule : ModuleBase<ShardedCommandContext>
    {
        [Command("info")]
        public async Task Info()
        {
            await ReplyAsync($"Displaying info for: {Context.Guild.Name}\n\n" +
                $"Shard: {Context.Client.GetShardFor(Context.Guild).ShardId}");
        }

        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Not implemented yet (I do nothing!)");
        }

        [Command("setprefix")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task SetPrefix(string newPrefix)
        {
            if(Program.guildPrefixes.ContainsKey(Context.Guild.Id))
            {
                Program.guildPrefixes[Context.Guild.Id] = newPrefix;
            }
            else
            {
                Program.guildPrefixes.Add(Context.Guild.Id, newPrefix);
            }

            try
            {
                using (SqlConnection server = new SqlConnection(Program.conn.ConnectionString))
                {
                    server.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE {Program.dbo}.GuildConfig SET Prefix = '{newPrefix}' WHERE GuildId = '{Context.Guild.Id}'", server);
                    int result = await cmd.ExecuteNonQueryAsync();
                    if (result <= 0)
                    {
                        cmd.CommandText = $"INSERT INTO {Program.dbo}.GuildConfig VALUES ('{Context.Guild.Id}','{newPrefix}')";
                    }
                }

                await Context.Channel.SendMessageAsync($"Prefix is now set to: {newPrefix}");
            }
            catch (Exception ex)
            {
                await Context.Channel.SendMessageAsync("Looks like something went wrong. Make sure not to use `'` (single quote) as a prefix. The prefix has been reset back to the defualt prefix of `>`");
                if (Program.IsDebug) Console.WriteLine(ex.StackTrace);
                Program.guildPrefixes[Context.Guild.Id] = ">";
                throw ex;
            }
        }
    }
}
