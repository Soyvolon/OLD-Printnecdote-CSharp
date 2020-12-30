using Items.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Items
{
    public class Resource : ItemBase
    {
        /// <summary>
        /// Types of Resources
        /// </summary>
        public enum Type
        {
            Crafting,
            Misc
        }

        /// <summary>
        /// Value of the resource before supply and demand
        /// </summary>
        public int Value { get; set; }
        public Resource(long id) : base(id)
        {

        }

        public Resource(string name) : base(name) { }

        public Resource(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Resource(long itemId, string name, Rarity rarity) : base(itemId, name, rarity) { }

        /// <summary>
        /// Creates a base item by changing the variables to match those in the Item Database
        /// </summary>
        /// <param name="sqlConnString">Database Connection String</param>
        /// <param name="dbToAccess">Database Location. Ex: Prox.dbo.Items</param>
        /// <returns>True: Item Created\nFalse: No item found in Database</returns>
        public override bool CreateBaseItem(string sqlConnString, string dbToAccess)
        {
            if (GenerateBaseItem(sqlConnString, dbToAccess, out Resource item))
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
        protected bool AssignDefaultVars(Resource item)
        {
            Value = item.Value;

            return base.AssignDefaultVars(item);
        }
    }
}
