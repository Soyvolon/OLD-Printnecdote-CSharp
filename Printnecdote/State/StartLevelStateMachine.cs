using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Printnecdote.Game;
using Printnecdote.Game.Levels;

namespace Printnecdote.State
{
    class StartLevelStateMachine : StateMachine
    {
        public StartLevelStateMachine(ulong creatorId) : base(creatorId)
        {

        }

        protected void InitializeLevel(LevelBase level)
        {

        }

        public override bool UpdateState(ICommandContext context)
        {
            switch(state)
            {
                case -1: // Admin level creation
                    SendMsg("Admin Level Starter:\n" +
                        "[1] Run Test Combat\n" +
                        "[0] Exit", context);
                    state = -2;
                    break;
                case -2:
                    string msg = context.Message.Content.ToLower();
                    if(msg == "1")
                    {

                    }
                    else if (msg == "0")
                    {

                    }
                    break;
            }

            return false;
        }
    }
}
