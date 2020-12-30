using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Items;
using Items.Utility;

namespace Printnecdote.Game.Levels
{
    class LevelBase : State.StateMachine
    {
        protected bool SetupDone { get; set; }
        protected ITextChannel Channel { get; set; }
        protected OverwritePermissions ChannelPerms { get; set; }
        protected Dictionary<ulong, Player> ActivePlayers = new Dictionary<ulong, Player>();

        protected int CombatState { get; set; }
        protected List<LivingGameObject> CurrentTurnOrder { get; set; }
        protected List<LivingGameObject> CurrentEnemies { get; set; }
        protected int TurnCount { get; set; }
        protected ulong PlayersTurnId { get; set; }
        protected IUserMessage StatusMessage { get; private set; }
        protected List<ItemBase> Loot { get; set; }
        protected Dictionary<ulong, Score> Scores { get; set; }

        protected enum LootDistribution
        {
            Normal, // All loot gets spread evenely, no copies of items.
            Duplicate // All loot gets sent to each person.
        }

        /// <summary>
        /// Used to add a new enemy to the current enemies list.
        /// </summary>
        /// <param name="lgo"></param>
        protected virtual void AddToCurrentEnemies(LivingGameObject lgo)
        {
            lgo.DidFaint = OnFaint;
            CurrentEnemies.Add(lgo);
        }
           

        public LevelBase(ICommandContext context, ulong startedBy) : base(startedBy)
        {
            CurrentTurnOrder = new List<LivingGameObject>();
            CurrentEnemies = new List<LivingGameObject>();
            Loot = new List<ItemBase>();
            SetupDone = false;
            state = 0;
            CombatState = 0;
            InitializeDiscord(context);
        }

        private void InitializeDiscord(ICommandContext context)
        {
            Channel = context.Guild.CreateTextChannelAsync($"{context.User.Username}'s Party [Printnecdote]", x => x.Position = 0).GetAwaiter().GetResult();
            ChannelPerms = OverwritePermissions.DenyAll(Channel).
                 Modify(viewChannel: PermValue.Allow,
                 sendMessages: PermValue.Allow,
                 attachFiles: PermValue.Allow,
                 embedLinks: PermValue.Allow);
            Channel.AddPermissionOverwriteAsync(context.User, ChannelPerms);
            Channel.AddPermissionOverwriteAsync(context.Client.CurrentUser, ChannelPerms);
            Channel.AddPermissionOverwriteAsync(context.Guild.EveryoneRole, OverwritePermissions.DenyAll(Channel).Modify(viewChannel: PermValue.Allow));
        }

        protected void InitializeWorld()
        {
            Scores = new Dictionary<ulong, Score>();
            foreach(ulong uid in activeUsers)
            {
                ActivePlayers.Add(uid, Program.game.PlayerDict[uid]);
                Program.game.PlayerDict[uid].ResetLivingObject();
                Scores.Add(uid, new Score());
            }
        }

        protected void AddUserToChannel(IUser user)
        {
            Channel.AddPermissionOverwriteAsync(user, ChannelPerms);
        }

        public override bool UpdateState(ICommandContext context)
        {
            if (state == 0 || context.Channel.Id == Channel.Id)
            {
                switch (state)
                {
                    case 0:
                        Channel.SendMessageAsync($"Add party members with `>add <user> [user] [user]` (*at least one user, up to 3. Can be done multiple times.*)\n" +
                            $"Party Size: {activeUsers.Count}/4\n" +
                            $"Use `>start` to begin").GetAwaiter().GetResult();
                        state = 1;
                        break;
                    case 1:
                        if (context.Message.Content.ToLower().StartsWith(">add"))
                        {
                            foreach (ulong uid in context.Message.MentionedUserIds)
                            {
                                if (activeUsers.Count >= 4)
                                {
                                    // Party full.
                                    break;
                                }
                                else if (Program.game.PlayerDict.ContainsKey(uid) == false)
                                {
                                    SendMsg("User must have an account made before beign invited to a party", context);
                                    break;
                                }

                                IGuildUser user = context.Guild.GetUserAsync(uid).GetAwaiter().GetResult();
                                AddUserToChannel(user);
                                activeUsers.Add(uid);
                                SendMsg($"Invited (to actually add the invite at some point, for now it just adds): {user.Mention}", context);
                            }
                        }
                        else if (context.Message.Content.ToLower().StartsWith(">start"))
                        {
                            return true;
                        }
                        break;
                }
            }
            return false;
        }
        /// <summary>
        /// Runs a combat secuence between the actifve players and whatever eneimies are in the current stage
        /// </summary>
        /// <param name="context">Command Context</param>
        /// <param name="status">-1 = Player Loss. 0 = Still in Combat. 1 = Player Win</param>
        /// <returns>True if round is fhinished, false if still going</returns>
        protected virtual bool CombatSequence(ICommandContext context, [Range(-1, 1)] out int status)
        {
            status = 1;
            return true;
        }

        protected void UpdateTurnOrder()
        {
            CurrentTurnOrder = new List<LivingGameObject>();
            CurrentTurnOrder.AddRange(CurrentEnemies);
            CurrentTurnOrder.AddRange(ActivePlayers.Values);
            CurrentTurnOrder.Sort((x, y) => x.CurrentSpeed.CompareTo(y.CurrentSpeed));
        }

        protected string GetEncounterString()
        {
            string output = "";
            foreach(LivingGameObject lgo in CurrentEnemies)
            {
                output += "\nA **" + lgo.GetType().Name + "** named **" + lgo.Name + "**.";
            }
            return output;
        }

        protected bool AllParyMembersFainted()
        {
            foreach(Player p in ActivePlayers.Values.ToList())
            {
                if(!p.Fainted)
                {
                    return false;
                }
            }
            return true;
        }

        protected bool AllEnemiesFainted()
        {
            foreach(LivingGameObject lgo in CurrentEnemies)
            {
                if(!lgo.Fainted)
                {
                    return false;
                }
            }
            return true;
        }

        protected int LootDistState = 0;
        protected List<Player> LootPlayers = new List<Player>();
        protected List<ItemBase> LegendaryLoot = new List<ItemBase>();
        protected ItemBase DuplicatedLoot;

        protected bool DistributeLoot(ICommandContext context, LootDistribution distribution)
        {
           switch(LootDistState)
            {
                case 0:
                    switch (distribution)
                    {
                        case LootDistribution.Normal:
                            LootDistState = 1;
                            Loot.Sort((x, y) => x.ItemRarity.CompareTo(y.ItemRarity));
                            if(Loot.FindAll(x => x.ItemRarity == Rarity.Legendary).Count > 0)
                            {
                                LegendaryLoot.AddRange(Loot.FindAll(x => x.ItemRarity == Rarity.Legendary));
                                Loot.RemoveAll(x => LegendaryLoot.Contains(x));
                            }
                            else
                            {
                                SetDuplicatedLoot();
                            }

                            if(LegendaryLoot.Count > 0 || DuplicatedLoot != null)
                            {
                                SendDuplicatedLoot();   
                            }

                            LootPlayers = ActivePlayers.Values.ToList();
                            LootPlayers.Sort((x, y) => Scores[x.Id].GetTotalScore().CompareTo(Scores[y.Id].GetTotalScore()));
                            //ActivePlayers.OrderBy(x => Scores[x.Key]);
                            return DistributeLoot(context, distribution);
                            
                        case LootDistribution.Duplicate:
                            break;
                    }
                    break;
                case 1: // Begin normal loot distribution
                    if (Loot.Count > 0)
                    {
                        if (LootPlayers.Count > 0)
                        {
                            SendLootEmbed($"{LootPlayers[0].Name}'s Pick\nCommands: `>loot <#>`");
                            LootDistState = 2;
                        }
                        else
                        {
                            LootDistState = 3;
                            return DistributeLoot(context, distribution);
                        }
                    }
                    else
                    {
                        LootDistState = 4;
                        return DistributeLoot(context, distribution);
                    }
                    break;
                case 2: // Player at 0 in LootPlayers (Highest score that has not picked) gets to pick loot.
                    if(context.User.Id == LootPlayers[0].Id)
                    {
                        if(context.Message.Content.ToLower().StartsWith(">loot"))
                        {
                            if(int.TryParse(context.Message.Content.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1], out int num))
                            {
                                if (num < Loot.Count && num > 0)
                                {
                                    num--;
                                    LootPlayers[0].AddItemToInventory(Loot[num]);
                                    SendGameMsg($"**{LootPlayers[0].Name}** looted **{Loot[num].Name}**");
                                    Loot.RemoveAt(num);
                                    LootPlayers.RemoveAt(0);
                                    LootDistState = 1;
                                    DistributeLoot(context, distribution);
                                }
                                else
                                {
                                    SendGameMsg("Please select loot from the displayed loot. (number too large or below 1.)");
                                }
                            }
                            else
                            {
                                SendGameMsg("Must be a number whole after `>loot`");
                            }
                        }
                    }
                    break;
                case 3: // Distribute remaning loot randomly between each player.
                    Dictionary<Player, EmbedFieldBuilder> fields = new Dictionary<Player, EmbedFieldBuilder>();

                    foreach (Player p in ActivePlayers.Values.ToArray())
                    {
                        fields.Add(p, new EmbedFieldBuilder()
                        {
                            IsInline = false,
                            Name = $"{p.Name}'s Loot"
                        });
                    }

                    while (Loot.Count > 0)
                    {
                        foreach(Player p in fields.Keys)
                        {
                            int num = Program._rand.Next(Loot.Count);
                            p.AddItemToInventory(Loot[num]);
                            fields[p].Value += $"[{GetRarityString(Loot[num])}] {Loot[num].Name}\n";
                            Loot.Remove(Loot[num]);
                        }
                    }

                    var embed = new EmbedBuilder()
                    {
                        Title = "Random Loot Assignment",
                        Color = Color.Gold,
                        Fields = fields.Values.ToList()
                    };

                    SendGameMsg(embed.Build());
                    LootDistState = 4;
                    return DistributeLoot(context, distribution);
                case 4:
                    SendGameMsg("All Loot Distributed.");
                    return true;
            }
            return false;
        }

        protected void SendDuplicatedLoot()
        {
            Dictionary<Player, EmbedFieldBuilder> dupeFields = new Dictionary<Player, EmbedFieldBuilder>();

            foreach (Player p in ActivePlayers.Values.ToArray())
            {
                dupeFields.Add(p, new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = $"{p.Name}'s Loot"
                });
            }

            foreach (Player p in dupeFields.Keys)
            {
                if (DuplicatedLoot != null)
                {
                    dupeFields[p].Value = $"[{GetRarityString(DuplicatedLoot)}] {DuplicatedLoot.Name}";
                    p.AddItemToInventory(DuplicatedLoot);
                }
                else if (LegendaryLoot.Count > 0)
                {
                    foreach (ItemBase item in LegendaryLoot)
                    {
                        dupeFields[p].Value += $"[{GetRarityString(item)}] {item.Name}";
                        p.AddItemToInventory(item);
                    }
                }
            }

            var dupeEmbed = new EmbedBuilder()
            {
                Title = "Duplicated Loot Assignment",
                Color = Color.Gold,
                Fields = dupeFields.Values.ToList()
            };

            SendGameMsg(dupeEmbed.Build());
        }

        protected void SendLootEmbed(string msg = "")
        {
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
            int count = 1;
            
            foreach(ItemBase item in Loot)
            {
                fields.Add(new EmbedFieldBuilder()
                {
                    IsInline = false,
                    Name = $"[{count++}][{GetRarityString(item)}]",
                    Value = $"{item.Name}"
                });
            }

            List<EmbedBuilder> builders = new List<EmbedBuilder>();
            int fieldCount = 20;
            for (int i = 0; i < fields.Count; i += fieldCount)
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Loot",
                    Description = msg ?? "",
                    Fields = fields.GetRange(i, (i + fieldCount >= fields.Count) ? fields.Count - i : fieldCount),
                    Color = Color.Gold
                };

                builders.Add(embed);
            }

            foreach(EmbedBuilder embed in builders)
            {
                try
                {
                    SendGameMsg(embed.Build());
                }
                catch (Exception ex)
                {
                    Program.prog.LogAsync(new LogMessage(LogSeverity.Error, "SendLootEmbed", ex.Message, ex));
                }
            }

        }

        private void SetDuplicatedLoot()
        {
            bool done = false;
            int dupeState = 0;
            while(!done)
            {
                switch(dupeState)
                {
                    case 0:
                        if (Loot.FindAll(x => x.ItemRarity == Rarity.UltraRare).Count == 1)
                        {
                            DuplicatedLoot = Loot.Find(x => x.ItemRarity == Rarity.UltraRare);
                            done = true;
                        }
                        else
                        {
                            dupeState = 1;
                        }
                        break;
                    case 1:
                        if (Loot.FindAll(x => x.ItemRarity == Rarity.Rare).Count == 1)
                        {
                            DuplicatedLoot = Loot.Find(x => x.ItemRarity == Rarity.Rare);
                            done = true;
                        }
                        else
                        {
                            dupeState = 2;
                        }
                        break;
                    case 2:
                        if (Loot.FindAll(x => x.ItemRarity == Rarity.Uncommon).Count == 1)
                        {
                            DuplicatedLoot = Loot.Find(x => x.ItemRarity == Rarity.Uncommon);
                            done = true;
                        }
                        else
                        {
                            dupeState = 3;
                        }
                        break;
                    case 3:
                        if (Loot.FindAll(x => x.ItemRarity == Rarity.Common).Count == 1)
                        {
                            DuplicatedLoot = Loot.Find(x => x.ItemRarity == Rarity.Common);
                            done = true;
                        }
                        else
                        {
                            dupeState = 4;
                        }
                        break;
                    case 4:
                        DuplicatedLoot = null;
                        done = true;
                        break;
                }
            }

            Loot.Remove(DuplicatedLoot);
        }

        protected virtual void OnFaint(LivingGameObject lgo)
        {
            // Add all enemy items to the inventory by default.
            Loot.AddRange(lgo.GetInventoryItems());
        }

        private string GetRarityString(ItemBase item)
        {
            switch(item.ItemRarity)
            {
                case Rarity.Common:
                    return "Common";
                case Rarity.Uncommon:
                    return "Uncommon";
                case Rarity.Rare:
                    return "Rare";
                case Rarity.UltraRare:
                    return "Ultra Rare";
                case Rarity.Legendary:
                    return "Legendary";
            }

            return "";
        }

        private string GetLootString(ItemBase item)
        {
            switch(item)
            {
                case Armor i:
                    return $"";
                case Consumable i:
                    break;
                case Necklace i:
                    break;
                case Resource i:
                    break;
                case Ring i:
                    break;
                case Weapon i:
                    break;
            }

            return "";
        }

        private string GetModifierStrings(ItemBase item)
        {
            switch(item)
            {
                case Armor i:
                    break;
                case Necklace i:
                    break;
                case Ring i:
                    break;
                case Weapon i:
                    break;
            }


            return "No Modifiers";
        }

        protected void SendScoreboardEmbed(string msg = "")
        {
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

            foreach (Player p in ActivePlayers.Values.ToArray())
            {
                fields.Add(new EmbedFieldBuilder()
                {
                    IsInline = true,
                    Name = p.Name + "'s Score",
                    Value = Scores[p.Id].GetTotalScore()
                });                  
            }

            var embed = new EmbedBuilder()
            {
                Title = "Scoreboard",
                Color = Color.Green,
                Fields = fields
            };


            if(msg != "")
            {
                SendGameMsg(msg, embed.Build());
            }
            else
            {
                SendGameMsg(embed.Build());
            }
        }

        protected void SendGameMsg(string msg)
        {
            Channel.SendMessageAsync(msg).GetAwaiter().GetResult();
        }

        protected void SendGameMsg(Embed embed)
        {
            Channel.SendMessageAsync(embed: embed).GetAwaiter().GetResult();
        }

        protected void SendGameMsg(string msg, Embed embed)
        {
            Channel.SendMessageAsync(msg, embed: embed).GetAwaiter().GetResult();
        }

        protected void SendStatusMsg(Embed embed, string text = "")
        {
            if (text != "")
            {
                Channel.SendMessageAsync(text).GetAwaiter().GetResult();
            }

            StatusMessage = Channel.SendMessageAsync(embed: embed).GetAwaiter().GetResult();

        }

        protected void UpdateStatusMessage(Embed embed = null, string text = "")
        {
            if(embed == null)
            {
                embed = StatusMessage.Embeds.ToList()[0].ToEmbedBuilder().Build();
            }
            Channel.DeleteMessageAsync(StatusMessage.Id).GetAwaiter().GetResult();
            SendStatusMsg(embed, text);
        }

        protected void SendStatusMsg(string embedCaption = "", string textMsg = "")
        {
            try
            {
                List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
                for(int i = 0; i < CurrentEnemies.Count; i++)
                {
                    LivingGameObject lgo = CurrentEnemies[i];
                    fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = $"[{i + 1}] {lgo.Name}",
                        Value = GetEnemyValueString(lgo)
                    });
                }

                bool isFirstPlayer = true;

                foreach (Player lgo in ActivePlayers.Values.ToList())
                {
                    fields.Add(new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = lgo.Name,
                        Value = GetPartyValueString(lgo)
                    });

                    if(isFirstPlayer)
                    {
                        isFirstPlayer = false;
                        fields[fields.Count - 1].IsInline = false;
                    }
                }

                var embed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        Name = $"Turn: {TurnCount}"
                    },
                    Color = Color.Blue,
                    Fields = fields,
                    Description = embedCaption
                };

                if (StatusMessage == null)
                {
                    SendStatusMsg(embed.Build(), textMsg);
                }
                else
                {
                    UpdateStatusMessage(embed.Build(), textMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
        }

        private string GetEnemyValueString(LivingGameObject lgo)
        {
            return $"HP: **{lgo.CurrentHealth}**/{lgo.MaxHealth}";
        }

        private string GetPartyValueString(LivingGameObject p)
        {
            return GetEnemyValueString(p) + $"\n" +
                $"MP: **{p.CurrentMagic}**/{p.MaxMagic}\n" +
                $"Speed: **{p.CurrentSpeed}**";
        }

        public void SaveGame()
        {
            try
            {
                foreach(Player p in ActivePlayers.Values)
                {
                    Program.game.SavePlayerData(p);
                }
            }
            catch
            {
                // catch is in SavePlayerData
            }
        }
    }
}
