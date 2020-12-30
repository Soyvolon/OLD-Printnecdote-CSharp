using Discord.WebSocket;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;

namespace Printnecdote.State
{
    public class StateMachine
    {
        protected int maxPlayers = 0;
        protected int state;
        protected ulong createdById;

        public readonly List<ulong> activeUsers = new List<ulong>();

        public StateMachine(ulong createdById)
        {
            state = 0;
            this.createdById = createdById;
            activeUsers.Add(createdById);
        }

        public virtual bool UpdateState(ICommandContext context)
        {
            return true;
        }

        protected IUserMessage SendMsg(string toSend, ICommandContext context)
        {
            return context.Channel.SendMessageAsync(toSend).GetAwaiter().GetResult();
        }

        protected IUserMessage SendMsg(Embed toEmbed, ICommandContext context)
        {
            return context.Channel.SendMessageAsync(embed: toEmbed).GetAwaiter().GetResult();
        }

        protected IUserMessage SendMsg(string toSend, Embed toEmbed, ICommandContext context)
        {
            return context.Channel.SendMessageAsync(toSend, embed: toEmbed).GetAwaiter().GetResult();
        }

        public void AddActiveUser(ulong id)
        {
            activeUsers.Add(id);
        }

        public void RemoveActiveUser(ulong id)
        {
            activeUsers.Remove(id);
        }
    }
}
