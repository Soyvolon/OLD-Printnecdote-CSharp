using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Printnecdote.State;
using System.Collections.Generic;
using System.Linq;
using Printnecdote.Game.Levels;

namespace Printnecdote.Services
{
    class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordShardedClient _discord;
        private readonly IServiceProvider _services;

        // State Machine Variables
        private readonly static List<StateMachine> stateMachines = new List<StateMachine>();
        //private readonly static List<long> activeIds = new List<long>();

        public static string GetGuildPrefix(ShardedCommandContext context)
        {
            if (Program.guildPrefixes.ContainsKey(context.Guild.Id))
            {
                return Program.guildPrefixes[context.Guild.Id];
            }
            return ">";
        }

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordShardedClient>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message))
                return;
            if (message.Source != MessageSource.User)
                return;

            var context = new ShardedCommandContext(_discord, message);

            // This value holds the offset where the prefix ends
            var argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos) && !message.HasStringPrefix(GetGuildPrefix(context), ref argPos))
            {
                CheckStateMachines(context);
                return;
            }
            else if (!Program.GameLoaded && !Program.BotAdmins.Contains(rawMessage.Author.Id))
            {
                // Send a please wait message.
                await rawMessage.Channel.SendMessageAsync("The bot is still loading, please wait before attempting commands.");
                return;
            }

            // A new kind of command context, ShardedCommandContext can be utilized with the commands framework

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified)
            {
                if(CheckStateMachines(context))
                    return;
            }

            // the command was succesful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
                return;

            // the command failed, let's notify the user that something happened.
            // TODO: Rework error messages
            await context.Channel.SendMessageAsync($"error: {result.ToString()}");
        }

        #region State Machine Handling

        // Looks at active state machines and compares if the user is inside of one.
        private bool CheckStateMachines(ICommandContext context)
        {
            StateMachine stateMachine = GetStateMachine(context.User.Id);
            if(stateMachine != null && stateMachine.UpdateState(context))
            {
                RemoveFromStateMachines(stateMachine);
                switch(stateMachine) // After level save game for all levels
                {
                    case LevelBase level:
                        level.SaveGame();
                        break;
                }
                return true; // user fhinished state machine
            }
            else if (stateMachine == null) // User did not interact with a state machine
            {
                context.Channel.SendMessageAsync("Command not found.").GetAwaiter().GetResult();
                return false;
            }
            else
            {
                return true; // User did interact with a state machine
            }
        }

        public static StateMachine GetStateMachine(ulong msgSenderId)
        {
            return stateMachines.First(x => x.activeUsers.Contains(msgSenderId));
        }

        public static void RemoveFromStateMachines(StateMachine state)
        {
            stateMachines.Remove(state);
        }

        public static void AddToStateMachines(StateMachine state, ICommandContext context)
        {
            // TODO: Ensure play is not already in another state machine
            stateMachines.Add(state);
            state.UpdateState(context);
        }

        #endregion

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }
    }
}
