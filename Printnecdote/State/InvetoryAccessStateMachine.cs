using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Items;
using static Printnecdote.State.InventoryAccessStateMachine.Filter;
using Printnecdote.Game;
using Items.Utility;
using Items.Modifiers;

namespace Printnecdote.State
{
    public class InventoryAccessStateMachine : StateMachine
    {
        public class Filter
        {
            public enum PrimaryFilter
            {
                None,
                Armor,
                Consumable,
                Necklace,
                Resource,
                Ring,
                Weapon
            }

            public enum Sort
            {
                None,
                Name,

            }

            public enum SortType
            {
                Ascending,
                Descending
            }

            public PrimaryFilter Primary { get; set; }
            public Sort SortBy { get; set; }
            public SortType SortDirection { get; set; }

            public Filter(PrimaryFilter primary = PrimaryFilter.None, Sort sortBy = Sort.None, SortType sortDirection = SortType.Ascending)
            {
                Primary = primary;
                SortBy = sortBy;
                SortDirection = sortDirection;
            }
        }

        protected readonly int _itemsPerPage = 20;
        protected readonly int _columns = 2;

        protected Filter lastFilter = null;
        protected Filter filter = new Filter();
        protected IUserMessage invEmbedMsg = null;
        protected IUserMessage itemDescMsg = null;
        protected int pageCount = 0;
        protected int currentPage = 0;

        public InventoryAccessStateMachine(ulong createdBy) : base(createdBy)
        {

        }

        public override bool UpdateState(ICommandContext context)
        {
            switch(state)
            {
                case 0:
                    DisplayInventoryEmbed(context);
                    state = 1;
                    return UpdateState(context);
                case 1:
                    return true;
            }

            return false;
        }

        private void DisplayInventoryEmbed(ICommandContext context)
        {
            Player player = Program.game.PlayerDict[activeUsers[0]];


            if (invEmbedMsg != null)
            { // Clear old embed if it exsits.
                invEmbedMsg.DeleteAsync().GetAwaiter().GetResult();
            }

            // Get list of items to display. use the filter for this part.
            List<ItemBase> itemsToDisplay = new List<ItemBase>();

            // Do this if the filter has changed
            if (lastFilter == null || lastFilter != filter)
            {
                currentPage = 0;

                switch (filter.Primary)
                {
                    case PrimaryFilter.None:
                        itemsToDisplay = player.GetInventoryItems().ToList();
                        break;
                    case PrimaryFilter.Armor:
                        player.GetInventoryItems<Armor>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                    case PrimaryFilter.Consumable:
                        player.GetInventoryItems<Consumable>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                    case PrimaryFilter.Necklace:
                        player.GetInventoryItems<Necklace>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                    case PrimaryFilter.Resource:
                        player.GetInventoryItems<Resource>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                    case PrimaryFilter.Ring:
                        player.GetInventoryItems<Ring>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                    case PrimaryFilter.Weapon:
                        player.GetInventoryItems<Weapon>().ForEach(x => itemsToDisplay.Add(x));
                        break;
                }


                switch (filter.SortBy)
                {
                    case Sort.None:
                        // NANI!?! theres nothing here!
                        break;
                    case Sort.Name:
                        itemsToDisplay.Sort((x, y) => x.Name.CompareTo(y.Name));
                        break;
                }



                switch (filter.SortDirection)
                {
                    case SortType.Ascending:
                        // Do nothing here. Already in correct order.
                        break;
                    case SortType.Descending:
                        itemsToDisplay.Reverse();
                        break;
                }

                pageCount = itemsToDisplay.Count / _itemsPerPage;
                lastFilter = filter;
            }

            // Organize items into fields. Set page count here.
            List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

            int columBreakOn = _itemsPerPage / _columns;


            for (int i = _itemsPerPage * currentPage; i < _itemsPerPage && i < itemsToDisplay.Count; i++)
            {
                var field = new EmbedFieldBuilder()
                {
                    IsInline = true,
                    Name = GetItemTitleString(i, itemsToDisplay[i]),
                    Value = GetShortItemDesc(itemsToDisplay[i])
                };

                fields.Add(field);
            }

            List<List<EmbedFieldBuilder>> organizer = new List<List<EmbedFieldBuilder>>();
            List<EmbedFieldBuilder> toSend = new List<EmbedFieldBuilder>();

            for (int i = 0; i < _columns; i++)
            {
                organizer.Add(new List<EmbedFieldBuilder>());
            }

            int colNum = 0;
            int itemColMax = columBreakOn - 1;

            for (int i = 0; i < fields.Count; i++)
            {
                organizer[colNum].Add(fields[i]);
                
                if(i == itemColMax)
                {
                    colNum++;
                }
            }

            for(int i = 0; i < columBreakOn; i++)
            {
                for(int x = 0; x < organizer.Count; x++)
                {
                    if(i < organizer[x].Count)
                    {
                        if (x == 0)
                        {
                            organizer[x][i].IsInline = true;
                        }

                        toSend.Add(organizer[x][i]);
                    }
                }
            }

            var embed = new EmbedBuilder()
            {
                Title = $"{player.Name}{(player.Name.TrimEnd().Last() == 's' ? "'" : "'s")} Inventory",
                Description = $"",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Page: {Format.Bold(currentPage.ToString())} of {Format.Bold(pageCount.ToString())}",
                }
            };

            embed.Fields = toSend;

            invEmbedMsg = SendMsg(embed.Build(), context);
        }

        private string GetItemTitleString(int pos, ItemBase item)
        {
            return $"[{pos + 1}]{GetRarityString(item)} {item.Name}";
        }

        private string GetRarityString(ItemBase item)
        {
            switch (item.ItemRarity)
            {
                case Rarity.Common:
                    return "[C]";
                case Rarity.Uncommon:
                    return "[U]";
                case Rarity.Rare:
                    return "[R]";
                case Rarity.UltraRare:
                    return "[R+]";
                case Rarity.Legendary:
                    return "[L]";
            }

            return "";
        }

        private string GetShortItemDesc(ItemBase item)
        {
            switch(item)
            {
                case Armor i:
                    return $"ARM: {i.BaseArmor}\tDUR: {i.Durability}\tARM-MODs: {(i.ArmorModifiers.Count > 0 ? "YES" : "NO")}";
                case Weapon i:
                    return $"DMG: {i.BaseDamage}\tDUR: {i.Durability}\tDMG-MODs: {(i.WeaponDamageModifers.Count > 0 ? "YES" : "NO" )}";
                case Necklace i:
                    PlayerModifiers mod = i.Modifiers.First(x => x.Value == i.Modifiers.Values.Max()).Key;
                    return $"{ItemBase.GetModifierShorthandString(mod, i.Modifiers[mod])} + {(i.Modifiers.Count != 1 ? (i.Modifiers.Count - 1).ToString() + "P-MODs" : "")}";
                case Ring i:
                    PlayerModifiers mod2 = i.Modifiers.First(x => x.Value == i.Modifiers.Values.Max()).Key;
                    return $"{ItemBase.GetModifierShorthandString(mod2, i.Modifiers[mod2])} + {(i.Modifiers.Count != 1 ? (i.Modifiers.Count - 1).ToString() + "P-MODs" : "")}";
                case Resource i:
                    return $"VAL: {i.Value.ToString("{0:n0}")}";
                case Consumable i:
                    return i.GetInfoString();
            }

            return "";
        }

        private void DisplayItemDescEmbed(ItemBase item)
        {

        }
    }
}
