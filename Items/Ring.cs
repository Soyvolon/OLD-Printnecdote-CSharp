using System;
using System.Collections.Generic;
using System.Text;
using Items.Modifiers;
using Items.Utility;
using Newtonsoft.Json;

namespace Items
{
    public class Ring : ItemBase
    {
        public Dictionary<PlayerModifiers, int> Modifiers { get; set; }
        public Ring(int id) : base(id)
        {
            Modifiers = new Dictionary<PlayerModifiers, int>();
        }

        public Ring(string name) : base(name)
        {
            Modifiers = new Dictionary<PlayerModifiers, int>();
        }

        public Ring(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Ring(long itemId, string name, Dictionary<PlayerModifiers, int> modifiers, Rarity rarity) : base(itemId, name, rarity)
        {
            Modifiers = modifiers;
            if (Modifiers == null)
            {
                Modifiers = new Dictionary<PlayerModifiers, int>();
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
            if (GenerateBaseItem(sqlConnString, dbToAccess, out Ring item))
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
        protected bool AssignDefaultVars(Ring item)
        {
            Modifiers = item.Modifiers;

            return base.AssignDefaultVars(item);
        }
    }
}
