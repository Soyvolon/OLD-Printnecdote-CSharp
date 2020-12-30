using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Items.Modifiers;
using Items.Utility;

namespace Items
{
    public class Weapon : ItemBase
    {
        /// <summary>
        /// How many uses the item has
        /// </summary>
        public int Durability { get; set; }
        /// <summary>
        /// Damage that the item does as a minimum
        /// </summary>
        public int BaseDamage { get; set; }
        /// <summary>
        /// Holds values for any damage modifiers the weapon has.
        /// </summary>
        public Dictionary<DamageModifiers, int> WeaponDamageModifers { get; set; }

        public Weapon(long id) : base(id)
        {
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        public Weapon(string name) : base(name)
        {
            WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
        }

        public Weapon(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Weapon(long itemId, string name, int durability, int baseDamage, Dictionary<DamageModifiers, int> weaponDamageModifers, Rarity rarity) : base(itemId, name, rarity)
        {
            Durability = durability;
            BaseDamage = baseDamage;
            WeaponDamageModifers = weaponDamageModifers;
            if(WeaponDamageModifers == null)
            {
                WeaponDamageModifers = new Dictionary<DamageModifiers, int>();
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
            if (GenerateBaseItem(sqlConnString, dbToAccess, out Weapon item))
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
        protected bool AssignDefaultVars(Weapon item)
        {
            Durability = item.Durability;
            BaseDamage = item.BaseDamage;
            WeaponDamageModifers = item.WeaponDamageModifers;

            return base.AssignDefaultVars(item);
        }
    }
}
