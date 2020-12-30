using Items;
using Items.Modifiers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Printnecdote.Game.AI
{
    public static class HostileBaseAi
    {
        // Cumaltive hit chances that the player in the list will be targeted. Works when player list is 4 or less.
        // First number is the first player, last is the last player. if there is one player, gaurenteed target.
        [Range(0.0, 1.0)] private static readonly double[] hitChance4 = { 0.4, 0.7, 0.9, 1.0 };
        [Range(0.0, 1.0)] private static readonly double[] hitChance3 = { 0.45, 0.8, 1.0};
        [Range(0.0, 1.0)] private static readonly double[] hitChance2 = { 0.65, 1.0};
        /// <summary>
        /// Targets an enemy. If self targeted, assume consumable or self attack has already been completed.
        /// </summary>
        /// <param name="enemies">List of player avalible to target</param>
        /// <returns>Object that has been targeted</returns>
        public static LivingGameObject GetTarget(LivingGameObject me, List<Player> enemies)
        {
            if(me.CurrentHealth <= me.MaxHealth * .25 && HasHealthIncreasers(me))
            {
                // Implement use consumable
            }
            else
            {
                enemies.Sort((x, y) => x.CurrentHealth.CompareTo(y.CurrentHealth));
                double hitP = Program._rand.NextDouble();
                for(int i = 0; i < enemies.Count; i++)
                {
                    switch(enemies.Count)
                    {
                        case 1:
                            return enemies[i];
                        case 2:
                            if(hitP < hitChance2[i])
                            {
                                return enemies[i];
                            }
                            break;
                        case 3:
                            if(hitP < hitChance3[i])
                            {
                                return enemies[i];
                            }
                            break;
                        case 4:
                            if(hitP < hitChance4[i])
                            {
                                return enemies[i];
                            }
                            break;
                    }
                }
            }

            return null;
        }

        private static bool HasHealthIncreasers(LivingGameObject me)
        {
            List<Consumable> consumeables = me.GetInventoryItems<Consumable>();
            consumeables.FirstOrDefault(x => x.Modifiers.ContainsKey(ConsumableModifiers.Health) && x.Modifiers[ConsumableModifiers.Health] > 0);
            if (consumeables != default)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
