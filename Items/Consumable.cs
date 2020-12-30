using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Items.Modifiers;
using Items.Utility;
using Newtonsoft.Json;

namespace Items
{
    public class Consumable : ItemBase
    {
        public Dictionary<ConsumableModifiers, int> Modifiers { get; private set; }
        public Consumable(long id) : base(id)
        {
            Modifiers = new Dictionary<ConsumableModifiers, int>();
        }

        public Consumable(string name) : base(name)
        {
            Modifiers = new Dictionary<ConsumableModifiers, int>();
        }

        public Consumable(string name, long id, Rarity rarity) : base(id, name, rarity) { }

        [JsonConstructor]
        public Consumable(long itemId, string name, Dictionary<ConsumableModifiers, int> modifiers, Rarity rarity) : base(itemId, name, rarity)
        {
            Modifiers = modifiers;
            if (this.Modifiers == null)
            {
                this.Modifiers = new Dictionary<ConsumableModifiers, int>();
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
            if (GenerateBaseItem(sqlConnString, dbToAccess, out Consumable item))
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
        protected bool AssignDefaultVars(Consumable item)
        {
            Modifiers = item.Modifiers;

            return base.AssignDefaultVars(item);
        }

        public string GetInfoString()
        {
            string output = "";
            ConsumableModifiers[] Modkeys = Modifiers.Keys.ToArray();

            for(int i = 0; i < Modkeys.Count(); i++)
            {
                output += GetModifierShorthandString(Modkeys[i], Modifiers[Modkeys[i]]);

                if(i != Modkeys.Count() - 1)
                {
                    output += " | ";
                }
            }

            return output;
        }
    }
}
