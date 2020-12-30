using Printnecdote.Game.Users;
using Items;
using System;
using System.Collections.Generic;
using System.Text;

namespace Printnecdote.Game.NPC
{
    class Skeleton : LivingGameObject
    {
        public Skeleton() : this("Skeleton") { }

        // Has an option for combat testing mode
        public Skeleton(string name, bool forTesting = false) : base(name)
        {
            Inv = new Inventory();
            if(forTesting) { GenerateTestItems(); }
        }

        private void GenerateRandomItems()
        {

        }

        /// <summary>
        /// Setsup the mob for a test session
        /// </summary>
        private void GenerateTestItems()
        {
            Inv.AddToInventory(new Armor(9900));
            Inv.AddToInventory(new Armor(9901));
            Inv.AddToInventory(new Armor(9902));
            Inv.AddToInventory(new Armor(9903));
            Inv.AddToInventory(new Ring(9940));
            Inv.AddToInventory(new Weapon(9950));
            foreach(ItemBase item in Inv.InventoryList)
            {
                item.CreateBaseItem(Program.conn.ConnectionString, $"{Program.dbo}.ItemConfig");
            }

            foreach(Armor item in GetInventoryItems<Armor>())
            {
                EquipItem(item);
            }

            EquipItem(Inv.InventoryList.Find(x => x.ItemId == 9940), 1);
            EquipItem(Inv.InventoryList.Find(x => x.ItemId == 9950), 1);
        }
    }
}
