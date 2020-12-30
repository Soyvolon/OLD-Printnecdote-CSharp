using System;
using System.Collections.Generic;
using System.Text;
using static Printnecdote.Game.Users.Utilities;
using Items;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Printnecdote.Game.Users
{
    public class Inventory
    {
        public Weapon PrimaryWeapon { get; private set; }
        public Weapon SecondaryWeapon { get; private set; }
        public Necklace Necklace { get; private set; }
        public Ring[] Rings { get; private set; }
        public Armor HeadArmor { get; private set; }
        public Armor ChestArmor { get; private set; }
        public Armor LegArmor { get; private set; }
        public Armor FeetArmor { get; private set; }
        public List<ItemBase> InventoryList { get; private set; }

        public Inventory()
        {
            InventoryList = new List<ItemBase>();
            Rings = new Ring[2];
        }

        /// <summary>
        /// Add an item to the players Inventory
        /// </summary>
        /// <param name="item">item to add</param>
        /// <returns>True if item was added sucessfuly</returns>
        public bool AddToInventory(ItemBase item)
        {
            if(InventoryList.Any(i => ReferenceEquals(i, item)))
            {
                // Exact item is already in inventory. Can't have dupes now can we.
                return false;
            }

            InventoryList.Add(item);
            return true;
        }

        /// <summary>
        /// Removes the provided item form the inventory
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True: Item Removed\nFalse: Item not found in inventory</returns>
        public bool RemoveFromInventory(ItemBase item)
        {
            if(InventoryList.Contains(item))
            {
                InventoryList.Remove(item);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Equips an item
        /// </summary>
        /// <param name="item">Item to equip</param>
        /// <param name="replacedItem">Item that was replaced. Null if no item was replaced.</param>
        /// <param name="slot">Slot to put item in, if applicable</param>
        /// <returns>True if item equiped, false if not.</returns>
        public bool EquipItem(ItemBase item, out ItemBase replacedItem, [Range(-1,1)] int slot = -1)
        {
            if (InventoryList.Contains(item))
            {
                switch (item)
                {
                    case Weapon w:
                        if (slot != -1)
                        {
                            return EquipWeapon(w, slot, out replacedItem);
                        }
                        else
                        {
                            replacedItem = null;
                            return false;
                        }
                    case Armor a:
                        return EquipArmor(a, out replacedItem);
                    case Ring r:
                        if (slot != -1)
                        {
                            return EquipRing(r, slot, out replacedItem);
                        }
                        else
                        {
                            replacedItem = null;
                            return false;
                        }
                    case Necklace n:
                        return EquipNecklace(n, out replacedItem);
                }
            }
            replacedItem = null;
            return false;
        }

        private bool EquipWeapon(Weapon item, [Range(0,1)] int slot, out ItemBase replacedItem)
        {
            // TODO: More checks for specific weapon types
            switch(slot)
            {
                case 0: // Primary Weapon Slot
                    if (ReferenceEquals(PrimaryWeapon, item))
                    {
                        // Weapons are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(SecondaryWeapon, item))
                    {
                        // Weapon is in the other slot. Switch slots.
                        SecondaryWeapon = PrimaryWeapon;
                        PrimaryWeapon = item;
                        // Nothing was changed in the inventory, so retun null.
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Weapon is new. Repalce old Weapon.
                        replacedItem = PrimaryWeapon;
                        PrimaryWeapon = item;
                        return true;
                    }
                case 1: // Secondary Weapon Slot
                    if (ReferenceEquals(SecondaryWeapon, item))
                    {
                        // Weapons are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(PrimaryWeapon, item))
                    {
                        // Weapon is in the other slot. Switch slots.
                        PrimaryWeapon = SecondaryWeapon;
                        SecondaryWeapon = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Weapon is new. Repalce old Weapon.
                        replacedItem = SecondaryWeapon;
                        SecondaryWeapon = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        private bool EquipArmor(Armor item, out ItemBase replacedItem)
        {
            switch(item.Type)
            {
                case Armor.ArmorType.Head:
                    if (ReferenceEquals(HeadArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = HeadArmor;
                        HeadArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Chest:
                    if (ReferenceEquals(ChestArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = ChestArmor;
                        ChestArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Leg:
                    if (ReferenceEquals(LegArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = LegArmor;
                        LegArmor = item;
                        return true;
                    }
                case Armor.ArmorType.Feet:
                    if (ReferenceEquals(FeetArmor, item))
                    {
                        // Items are the same, fail.
                        replacedItem = null;
                        return false;
                    }
                    else
                    {
                        // Items are differnet, equip.
                        replacedItem = FeetArmor;
                        FeetArmor = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        private bool EquipRing(Ring item, [Range(0,1)] int slot, out ItemBase replacedItem)
        {
            switch (slot)
            {
                case 0: // Ring Slot 1
                    if (ReferenceEquals(Rings[0], item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(Rings[1], item))
                    {
                        // Ring is in the other slot. Switch slots.
                        Rings[1] = Rings[0];
                        Rings[0] = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Repalce old ring.
                        replacedItem = Rings[0];
                        Rings[0] = item;
                        return true;
                    }
                case 1: // Ring Slot 2
                    if (ReferenceEquals(Rings[1], item))
                    {
                        // Rings are the same. Fail.
                        replacedItem = null;
                        return false;
                    }
                    else if (ReferenceEquals(Rings[0], item))
                    {
                        // Ring is in the other slot. Switch slots.
                        Rings[0] = Rings[1];
                        Rings[1] = item;
                        replacedItem = null;
                        return true;
                    }
                    else
                    {
                        // Ring is new. Replace old ring.
                        replacedItem = Rings[1];
                        Rings[1] = item;
                        return true;
                    }
            }
            replacedItem = null;
            return false;
        }

        private bool EquipNecklace(Necklace item, out ItemBase replacedItem)
        {
            if (ReferenceEquals(Necklace, item))
            {
                // Necklace is the same. Fail.
                replacedItem = null;
                return false;
            }

            // Necklace is new. Replace old necklace
            replacedItem = Necklace;
            Necklace = item;
            return true;
        }
    }
}
