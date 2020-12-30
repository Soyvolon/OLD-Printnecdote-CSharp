using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Printnecdote.Game.Users;
using System.Linq;
using Items;
using Printnecdote.Game;
using Printnecdote.Game.Levels;
using Printnecdote.Services;
using Discord;
using Discord.WebSocket;

namespace Printnecdote.Modules
{
    //[RequireOwner]
    public class TestCommandsModule : ModuleBase<ShardedCommandContext>
    {
        private Weapon testWeapon = new Weapon(-1);
        private Weapon testWeapon2 = new Weapon(-1);
        private Weapon testWeapon3 = new Weapon(-1);
        private Armor testArmorHead = new Armor(-1);
        private Armor testArmorChest = new Armor(-1);
        private Armor testArmorLegs = new Armor(-1);
        private Armor testArmorFeet = new Armor(-1);
        private Necklace testNecklace = new Necklace(-1);
        private Necklace testNecklace2 = new Necklace(-1);
        private Ring testRing1 = new Ring(-1);
        private Ring testRing2 = new Ring(-1);
        private Ring testRing3 = new Ring(-1);

        private void genTestNames()
        {
            testWeapon.SetItemName("Test Weapon One");
            testWeapon2.SetItemName("Test Weapon Two");
            testWeapon3.SetItemName("Test Weapon Three");
            testArmorHead.SetItemName("Test Armor Head");
            testArmorHead.Type = Armor.ArmorType.Head;
            testArmorChest.SetItemName("Test Armor Chest");
            testArmorChest.Type = Armor.ArmorType.Chest;
            testArmorLegs.SetItemName("Test Armor Legs");
            testArmorLegs.Type = Armor.ArmorType.Leg;
            testArmorFeet.SetItemName("Test Armor Feet");
            testArmorFeet.Type = Armor.ArmorType.Feet;
            testNecklace.SetItemName("Test Necklace Uno");
            testNecklace2.SetItemName("Test Necklace Duex");
            testRing1.SetItemName("Test Ring I");
            testRing2.SetItemName("Test Ring II");
            testRing3.SetItemName("Test Ring III");
        }

        [Command("testEquip")]
        [RequireOwner]
        public async Task testEquipMethod()
        {
            await Context.Channel.SendMessageAsync("Starting...");
            genTestNames();
            Inventory inv = new Inventory();
            await Context.Channel.SendMessageAsync($"New Item Added to inventory: [True:{inv.AddToInventory(testWeapon)}]");
            inv.AddToInventory(testWeapon2); inv.AddToInventory(testWeapon3);
            inv.AddToInventory(testArmorChest); inv.AddToInventory(testArmorFeet); inv.AddToInventory(testArmorHead); inv.AddToInventory(testArmorLegs);
            inv.AddToInventory(testNecklace); inv.AddToInventory(testNecklace2);
            inv.AddToInventory(testRing1); inv.AddToInventory(testRing2); inv.AddToInventory(testRing3);
            ItemBase itemOut = null;
            await Context.Channel.SendMessageAsync($"Trying to add an item thats already in inventory: [False:{inv.AddToInventory(testWeapon)}]");
            await Context.Channel.SendMessageAsync("--");
            bool didEquip = inv.EquipItem(inv.InventoryList.FirstOrDefault(x => x.Name == testWeapon.Name), out itemOut, 0);
            await Context.Channel.SendMessageAsync($"\nAdd item from the inventory list to equiped items: [True:{didEquip}]");
            didEquip = inv.EquipItem(testWeapon2, out itemOut, 1);
            await Context.Channel.SendMessageAsync($"Add item for elsewhere that is in the invnetory list to equiped items: [True:{didEquip}]");
            didEquip = inv.EquipItem(new Weapon(-1), out itemOut, 1);
            await Context.Channel.SendMessageAsync($"Equip new item not in inventory list: [False:{didEquip}]");
            await Context.Channel.SendMessageAsync("--");
            didEquip = inv.EquipItem(testArmorChest, out itemOut);
            await Context.Channel.SendMessageAsync($"Equip chest armor: [True:{didEquip}]");
            await Context.Channel.SendMessageAsync($"Equip head armor: [True:{inv.EquipItem(testArmorHead, out itemOut)}]");
            await Context.Channel.SendMessageAsync($"Equip leg armor: [True:{inv.EquipItem(testArmorLegs, out itemOut)}]");
            await Context.Channel.SendMessageAsync($"Equip feet armor: [True:{inv.EquipItem(testArmorFeet, out itemOut)}]");
            await Context.Channel.SendMessageAsync($"Equip necklace: [True:{inv.EquipItem(testNecklace, out itemOut)}]");
            await Context.Channel.SendMessageAsync($"Equip ring1: [True:{inv.EquipItem(testRing1, out itemOut, 0)}]");
            await Context.Channel.SendMessageAsync($"Equip ring2: [True:{inv.EquipItem(testRing2, out itemOut, 1)}]");
            await Context.Channel.SendMessageAsync("--");
            await Context.Channel.SendMessageAsync($"Displaying Current Equipment:");
            await Context.Channel.SendMessageAsync($"W1: {inv.PrimaryWeapon.Name}");
            await Context.Channel.SendMessageAsync($"W2: {inv.SecondaryWeapon.Name}");
            await Context.Channel.SendMessageAsync($"AH: {inv.HeadArmor.Name}");
            await Context.Channel.SendMessageAsync($"AC: {inv.ChestArmor.Name}");
            await Context.Channel.SendMessageAsync($"AL: {inv.LegArmor.Name}");
            await Context.Channel.SendMessageAsync($"AF: {inv.FeetArmor.Name}");
            await Context.Channel.SendMessageAsync($"NL: {inv.Necklace.Name}");
            await Context.Channel.SendMessageAsync($"R1: {inv.Rings[0].Name}");
            await Context.Channel.SendMessageAsync($"R2: {inv.Rings[1].Name}");
            await Context.Channel.SendMessageAsync("--");
            await Context.Channel.SendMessageAsync($"Switching Weapons: [True:{inv.EquipItem(testWeapon, out itemOut, 1)}]");
            await Context.Channel.SendMessageAsync($"W1: {inv.PrimaryWeapon.Name}");
            await Context.Channel.SendMessageAsync($"W2: {inv.SecondaryWeapon.Name}");
            await Context.Channel.SendMessageAsync("--");
            await Context.Channel.SendMessageAsync($"Equiping item that is already there: [False:{inv.EquipItem(testNecklace, out itemOut)}]");
            await Context.Channel.SendMessageAsync($"Equiping new item: [True:{inv.EquipItem(testRing3, out itemOut, 0)}]");
            await Context.Channel.SendMessageAsync("--");
            await Context.Channel.SendMessageAsync($"Displaying Current Equipment:");
            await Context.Channel.SendMessageAsync($"W1: {inv.PrimaryWeapon.Name}");
            await Context.Channel.SendMessageAsync($"W2: {inv.SecondaryWeapon.Name}");
            await Context.Channel.SendMessageAsync($"AH: {inv.HeadArmor.Name}");
            await Context.Channel.SendMessageAsync($"AC: {inv.ChestArmor.Name}");
            await Context.Channel.SendMessageAsync($"AL: {inv.LegArmor.Name}");
            await Context.Channel.SendMessageAsync($"AF: {inv.FeetArmor.Name}");
            await Context.Channel.SendMessageAsync($"NL: {inv.Necklace.Name}");
            await Context.Channel.SendMessageAsync($"R1: {inv.Rings[0].Name}");
            await Context.Channel.SendMessageAsync($"R2: {inv.Rings[1].Name}");

            await Context.Channel.SendMessageAsync("...Complete");
        }

        private void AddToInv(Inventory inv)
        {
            inv.AddToInventory(testWeapon);
            inv.AddToInventory(testWeapon2); inv.AddToInventory(testWeapon3);
            inv.AddToInventory(testArmorChest); inv.AddToInventory(testArmorFeet); inv.AddToInventory(testArmorHead); inv.AddToInventory(testArmorLegs);
            inv.AddToInventory(testNecklace); inv.AddToInventory(testNecklace2);
            inv.AddToInventory(testRing1); inv.AddToInventory(testRing2); inv.AddToInventory(testRing3);
        }

        [Command("testgetinv")]
        [RequireOwner]
        public async Task TestGetInventoryType(string type)
        {
            Player p = new Player(1000000001);
            genTestNames(); AddToInv(p.Inv);
            switch(type.ToLower())
            {
                case "w":
                    await ReplyAsync(GetStringFromArray(p.GetInventoryItems<Weapon>().ToArray()));
                    break;
                case "a":
                    await ReplyAsync(GetStringFromArray(p.GetInventoryItems<Armor>().ToArray()));
                    break;
                case "n":
                    await ReplyAsync(GetStringFromArray(p.GetInventoryItems<Necklace>().ToArray()));
                    break;
                case "r":
                    await ReplyAsync(GetStringFromArray(p.GetInventoryItems<Ring>().ToArray()));
                    break;
            }
        }

        private string GetStringFromArray<T>(T[] list)
        {
            string output = "";
            foreach(T item in list)
            {
                switch(item)
                {
                    case ItemBase i:
                        output += i.Name + "\n";
                        break;
                    default:
                        output += item.ToString() + "\n";
                        break;
                }
            }
            return output;
        }

        [Command("testcombat")]
        public async Task TestCombatStarterCommand()
        {
            try
            {
                TestCombatLevel level = new TestCombatLevel(Context, Context.User.Id);
                CommandHandlingService.AddToStateMachines(level, Context);
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message + " " + ex.StackTrace);
            }
        }

        [Command("equiptestitems")]
        public async Task EquipTestItems(SocketUser user = null)
        {
            if(user == null)
            {
                user = Context.User;
            }

            if(Program.game.PlayerDict.ContainsKey(user.Id))
            {
                Player p = Program.game.PlayerDict[user.Id];
                p.AddItemToInventory(new Weapon(99900));
                foreach(ItemBase item in p.GetInventoryItems())
                {
                    item.CreateBaseItem(Program.conn.ConnectionString, $"{Program.dbo}.ItemConfig");
                    switch(item)
                    {
                        case Weapon _:
                            p.EquipItem(item, 0);
                            break;
                    }
                }



                await ReplyAsync("Added itesm to inv and equiped them.");
            }
            else
            {
                await ReplyAsync("User not found in game dict.");
            }
        }

        [Command("equiptestmulti")]
        public async Task EquipTestMulti(int amnt = 30, SocketUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            Player p = Program.game.PlayerDict[user.Id];

            for (int i = 0; i < amnt; i++)
            {
                p.AddItemToInventory(new Weapon(99900));
            }

            foreach (ItemBase item in p.GetInventoryItems())
            {
                item.CreateBaseItem(Program.conn.ConnectionString, $"{Program.dbo}.ItemConfig");
            }

            await ReplyAsync("done");
        }
    }
}
