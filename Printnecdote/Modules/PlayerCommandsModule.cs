using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Printnecdote.Services;
using Printnecdote.State;

namespace Printnecdote.Modules
{
    public class PlayerCommandsModule : ModuleBase<ShardedCommandContext>
    {
        [Command("+")]
        public async Task PressPlusToStart()
        {
            if(Program.game.PlayerDict.ContainsKey(Context.User.Id))
            {
                await Context.Channel.SendMessageAsync($"A Player Account has already created. If you would like to start over please use {CommandHandlingService.GetGuildPrefix(Context)}resetaccount");
                return;
            }
            PlayerCreationStateMachine state = new PlayerCreationStateMachine(Context.User.Id);
            CommandHandlingService.AddToStateMachines(state, Context);
        }

        [Command("resetaccount")]
        public async Task ResetPlayerAccount()
        {
            await ReplyAsync("command not implemented.");
        }

        [Command("inventory")]
        [Alias(new string[] { "inv" })]
        public async Task StartInventoryStateMachine()
        {
            await Context.Channel.TriggerTypingAsync();
            InventoryAccessStateMachine state = new InventoryAccessStateMachine(Context.User.Id);
            CommandHandlingService.AddToStateMachines(state, Context);
        }
    }
}
