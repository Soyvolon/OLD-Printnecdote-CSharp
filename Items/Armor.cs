using System;
using System.Collections.Generic;
using System.Text;
using Items.Modifiers;
using Items.Utility;
using Newtonsoft.Json;

namespace Items
{
    public class Armor : ItemBase
    {
        public enum ArmorType
        {
            Head,
            Chest,
            Leg,
            Feet
        }
        public ArmorType Type { get; set; }
        public int BaseArmor { get; set; }
        public int Durability { get; set; }
        public Dictionary<DamageModifiers, int> ArmorModifiers { get; set; }
        public Armor(int id) : base(id)
        {
            ArmorModifiers = new Dictionary<DamageModifiers, int>();

        }

        public Armor(string name) : base(name)
        {
            ArmorModifiers = new Dictionary<DamageModifiers, int>();
        }

        public Armor(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Armor(long ItemId, string Name, ArmorType Type, int BaseArmor, int Durability, Dictionary<DamageModifiers, int> ArmorModifiers, Rarity rarity) : base(ItemId, Name, rarity)
        {
            this.Type = Type;
            this.BaseArmor = BaseArmor;
            this.Durability = Durability;
            this.ArmorModifiers = ArmorModifiers;
            if (this.ArmorModifiers == null)
            {
                this.ArmorModifiers = new Dictionary<DamageModifiers, int>();
            }
        }



        /// <summary>
        /// Creates a base item by changing the variables to match those in the Item Database
        /// </summary>
        /// <param name="sqlConnString">Database Connection String</param>
        /// <param name="dbToAccess">Database Location. Ex: Prox.dbo.Items</param>
        /// <returns>True: Item Created\nFalse: No item found in Database</returns>
        public override bool CreateBaseItem(string sqlConnString, string dbToAccess)
        {
            if (GenerateBaseItem(sqlConnString, dbToAccess, out Armor item))
            {
                return AssignDefaultVars(item);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method used to assign all default variables from a base item to the new item
        /// </summary>
        /// <param name="item">Base Item form DB</param>
        /// <returns>True</returns>
        protected bool AssignDefaultVars(Armor item)
        {
            Type = item.Type;
            BaseArmor = item.BaseArmor;
            Durability = item.Durability;
            ArmorModifiers = item.ArmorModifiers;

            return base.AssignDefaultVars(item);
        }
    }
}
