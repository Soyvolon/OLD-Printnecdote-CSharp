using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Printnecdote.Game.AI;
using Printnecdote.Game.NPC;

namespace Printnecdote.Game.Levels
{
    class TestCombatLevel : LevelBase
    {
        public TestCombatLevel(ICommandContext context, ulong startedBy) : base(context, startedBy)
        {
            
        }

        public override bool UpdateState(ICommandContext context)
        {
            if (context.Channel.Id == Channel.Id || state == 0)
            {
                if (SetupDone == true)
                {
                    switch (state)
                    {
                        case 0:
                            SendGameMsg("Initalizing World.");
                            InitializeWorld();
                            state = 1;
                            UpdateState(context);
                            break;
                        case 1:
                            CurrentEnemies = new List<LivingGameObject>();
                            AddToCurrentEnemies(new Skeleton("The Bone Zone", true));
                            //AddToCurrentEnemies(new Skeleton("Mr. Bones", true));
                            SendGameMsg("You have stumbled upon: " + GetEncounterString());
                            CombatState = 0;
                            state = 2;
                            SendStatusMsg();
                            UpdateState(context);
                            break;
                        case 2:
                            if (CombatState == 0 || PlayersTurnId == context.User.Id)
                            {
                                if (CombatSequence(context, out int status))
                                {
                                    // TODO Win lose stuff
                                    SendStatusMsg();
                                    SendGameMsg($"Game over. Status code {(status == -1 ? "You Lose (-1)" : "You Win (1)")}\nStarting Loot Screen");
                                    if(status == -1)
                                    {
                                        state = 4;
                                    }
                                    else
                                    {
                                        state = 3;
                                    }
                                    UpdateState(context);
                                }
                            }
                            break;
                        case 3:
                            return DistributeLoot(context, LootDistribution.Normal);
                        case 4:
                            // you lost no loot
                            return true;
                    }
                }
                else
                {
                    SetupDone = base.UpdateState(context);
                    if(SetupDone)
                    {
                        state = 0;
                        UpdateState(context);
                    }
                }
            }
            return false;
        }

        

        // TODO Add checks for attacking fainted enemies and using fainted allies.

        /// <summary>
        /// Runs a combat secuence between the actifve players and whatever eneimies are in the current stage
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <param name="status">-1 = Player Loss. 0 = Still in Combat. 1 = Player Win</param>
        /// <returns>True if round is fhinished, false if still going</returns>
        protected override bool CombatSequence(ICommandContext context, [Range(-1, 1)] out int status)
        {
            if(AllEnemiesFainted())
            { // Players win
                status = 1;
                return true;
            }

            if (!AllParyMembersFainted())
            {
                if (CurrentTurnOrder.Count <= 0 || !CurrentTurnOrder[0].Fainted)
                {
                    switch (CombatState)
                    {

                        case 0: // Pre Move Setup
                            UpdateTurnOrder();
                            CombatState = 1;
                            return CombatSequence(context, out status);
                            break;
                        case 1: // Transition Phase between moves.
                            if (CurrentTurnOrder.Count <= 0)
                            {
                                CombatState = 0;
                                TurnCount++;
                                return CombatSequence(context, out status);
                            }
                            else
                            {
                                Task.Delay(5000).GetAwaiter().GetResult();
                                switch (CurrentTurnOrder[0])
                                {
                                    case Player p:
                                        PlayersTurnId = p.Id;
                                        CombatState = 2;
                                        SendStatusMsg($"{context.Guild.GetUserAsync(p.Id).GetAwaiter().GetResult().Mention}'s Turn\nCommands: `>attack`");
                                        break;
                                    case LivingGameObject _:
                                        CombatState = 4;
                                        return CombatSequence(context, out status);
                                        break;
                                }
                            }
                            break;
                        case 2: // Player Turn
                            string msg = context.Message.Content.ToLower();
                            if (msg.StartsWith(">attack") && msg != ">attack")
                            {
                                CombatState = 3;
                                return CombatSequence(context, out status);
                            }
                            else
                            {
                                switch (context.Message.Content.ToLower())
                                {
                                    case ">attack":
                                        CombatState = 3;
                                        SendGameMsg("Pick your target (number in the [#] for the enemys)");
                                        //context.Message.DeleteAsync();
                                        break;

                                }
                            }
                            break;
                        case 3:
                            Player activePlayer = CurrentTurnOrder[0] as Player;
                            List<string> msgParts = context.Message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (msgParts[0] == ">attack")
                            {
                                msgParts.RemoveAt(0);
                                //context.Message.DeleteAsync();
                            }

                            if (int.TryParse(msgParts[0], out int targetNum))
                            {
                                targetNum -= 1;
                                if (targetNum < CurrentEnemies.Count)
                                {

                                    if (activePlayer.Attack(CurrentEnemies[targetNum], out int dmg))
                                    {
                                        SendGameMsg($"**{activePlayer.Name}** attacked **{CurrentEnemies[targetNum].Name}** for **{dmg}** damage");
                                    }
                                    else
                                    {
                                        SendGameMsg($"**{activePlayer.Name}** missed the attack");
                                    }

                                    // Remove this player form the turn order as their turn has already happened
                                    CurrentTurnOrder.RemoveAt(0);
                                    CombatState = 1;
                                    return CombatSequence(context, out status);
                                }
                            }
                            else
                            {
                                SendGameMsg("Target Enemy Not Found");
                            }
                            
                            break;
                        case 4: // NPC Turn
                            //SendStatusMsg($"{CurrentTurnOrder[0].Name}'s Turn");
                            LivingGameObject target = HostileBaseAi.GetTarget(CurrentTurnOrder[0], ActivePlayers.Values.ToList());
                            if (target != CurrentTurnOrder[0])
                            {
                                if (CurrentTurnOrder[0].Attack(target, out int dmg))
                                {
                                    SendGameMsg($"**{CurrentTurnOrder[0].Name}** attacked **{target.Name}** for **{dmg}** damage");
                                }
                                else
                                {
                                    SendGameMsg($"**{CurrentTurnOrder[0].Name}** missed the attack");
                                }
                            }
                            else
                            {
                                SendGameMsg("Consumable stuff happened here. (Not implemented)");
                            }
                            // Remove this mob from the turn order as its turn has already happened
                            CurrentTurnOrder.RemoveAt(0);
                            CombatState = 1;
                            return CombatSequence(context, out status);
                            break;
                    }

                    status = 0;
                    return false;
                }
                else
                {
                    CurrentTurnOrder.RemoveAt(0);
                    return CombatSequence(context, out status);
                }
            }
            else
            { // All players are fainted
                status = -1;
                return true;
            }
        }
    }
}
