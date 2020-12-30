using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Printnecdote.Modules
{
    public class BotAdminCommandsModule : ModuleBase<ShardedCommandContext>
    {
        [Command("printtestitem")]
        public async Task printTestItem()
        {

            await Context.Channel.SendMessageAsync("test");
        }

        [Command("togglecommands")]
        [RequireOwner]
        public async Task ToggleBotCommands()
        {
            if (Program.GameLoaded)
            {
                Program.GameLoaded = false;
                await Program.prog.client.SetStatusAsync(UserStatus.DoNotDisturb);
                await ReplyAsync("Commands Disabled");
            }
            else
            {
                Program.GameLoaded = true;
                await Program.prog.client.SetStatusAsync(UserStatus.Online);
                await ReplyAsync("Commands Enabled");
            }
        }


    }
}
