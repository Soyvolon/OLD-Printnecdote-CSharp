using Items;
using Items.Modifiers;
using Newtonsoft.Json;
using Printnecdote.Game.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Printnecdote.Game
{
    public class LivingGameObject
    {

        /// <summary>
        /// Object Identification Name
        /// </summary>
        
        public string Name { get; set; }

        /// <summary>
        /// Object invtory object
        /// </summary>
        
        public Inventory Inv { get; protected set; }

        /// <summary>
        /// Holds the base health infromation before any equiped items or effects
        /// </summary>
        
        public int MaxHealth { get; protected set; }
        /// <summary>
        /// The current health after modifications are made.
        /// </summary>
        
        public int CurrentHealth { get; protected set; }
        /// <summary>
        /// Holds the base magic information before any equiped items or effects
        /// </summary>
        
        public int MaxMagic { get; protected set; }
        /// <summary>
        /// The current magic after modifications are made.
        /// </summary>
        
        public int CurrentMagic { get; protected set; }
        /// <summary>
        /// Holds the base speed information before any quiped items or effects
        /// </summary>
        
        public int MaxSpeed { get; protected set; }
        /// <summary>
        /// The current speed after modifications are made.
        /// </summary>
        
        public int CurrentSpeed { get; protected set; }
        /// <summary>
        /// Base armor value of the Object
        /// </summary>
        
        public int Armor { get; protected set; }
        /// <summary>
        /// Modifications to armor value
        /// </summary>
        
        public Dictionary<DamageModifiers, int> ArmorModifiers { get; protected set; }
        /// <summary>
        /// Base attack power of the Object
        /// </summary>
        
        public int AttackPower { get; protected set; }
        /// <summary>
        /// Modifications to attack power
        /// </summary>
        
        public Dictionary<DamageModifiers, int> AttackModifiers { get; protected set; }
        /// <summary>
        /// The level of the Living Game Object
        /// </summary>
        
        public int Level { get; private set; }
        /// <summary>
        /// Is the LivingGameObject fainted?
        /// </summary>
        
        public bool Fainted { get; private set; }
        /// <summary>
        /// Adds to the random hit chance of the user. Gains value from consecutive misses.
        /// </summary>
        
        protected double noMissModifier = 0.0;
        /// <summary>
        /// Delegate Base for running when an LGO faints
        /// </summary>
        public delegate void OnDeath(LivingGameObject lgo);
        /// <summary>
        /// Runs when the LGO Faints.
        /// </summary>
        [JsonIgnore]
        public OnDeath DidFaint { get; set; }

        public LivingGameObject(string newName)
        {
            AttackModifiers = new Dictionary<DamageModifiers, int>();
            ArmorModifiers = new Dictionary<DamageModifiers, int>();
            Name = newName;
            Inv = new Inventory();
            MaxHealth = 100; // DEV AT 10. DEFAULT AT 100.
            MaxMagic = 100;
            MaxSpeed = 10;
            DidFaint = delegate {/* Do Nothing, this method gets set by the Level. */};
            ResetLivingObject();
        }
        #region Inventory Management
        /// <summary>
        /// Gets an array of all items in the player Inventory
        /// </summary>
        /// <returns>List of items in the player inventory</returns>
        public ItemBase[] GetInventoryItems()
        {
            return Inv.InventoryList.ToArray();
        }
        /// <summary>
        /// Gets invetory items of the selected type
        /// </summary>
        /// <typeparam name="T">Type of items to get</typeparam>
        /// <returns>List of items of type T that are in the Inventory</returns>
        public List<T> GetInventoryItems<T>()
        {
            List<T> items = new List<T>();
            foreach (ItemBase item in GetInventoryItems())
            {
                switch (item)
                {
                    case T i:
                        items.Add(i);
                        break;
                }
            }
            return items;
        }
        
        [JsonConstructor]
        public LivingGameObject(string name, Inventory inv, int maxHealth, int currentHealth, int maxMagic, int currentMagic, int maxSpeed, int currentSpeed, int armor, Dictionary<DamageModifiers, int> armorModifiers, int attackPower, Dictionary<DamageModifiers, int> attackModifiers, int level, bool fainted, double noMissModifier) : this(name)
        {
            Inv = inv;
            MaxHealth = maxHealth;
            CurrentHealth = currentHealth;
            MaxMagic = maxMagic;
            CurrentMagic = currentMagic;
            MaxSpeed = maxSpeed;
            CurrentSpeed = currentSpeed;
            Armor = armor;
            ArmorModifiers = armorModifiers;
            AttackPower = attackPower;
            AttackModifiers = attackModifiers;
            Level = level;
            Fainted = fainted;
            this.noMissModifier = noMissModifier;

            if (ArmorModifiers == null)
            {
                ArmorModifiers = new Dictionary<DamageModifiers, int>();
            }
            if (AttackModifiers == null)
            {
                AttackModifiers = new Dictionary<DamageModifiers, int>();
            }
        }
        


        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void AddItemToInventory(ItemBase item)
        {
            Inv.AddToInventory(item);
        }
        /// <summary>
        /// Equips an item
        /// </summary>
        /// <param name="item">Item to equip</param>
        public void EquipItem(ItemBase item, [Range(-1, 1)] int slot = -1)
        {
            ItemBase replacedItem = null; // Holds data about the replaced item
            if (Inv.EquipItem(item, out replacedItem, slot))
            {
                switch (item)
                {
                    case Ring i:
                        foreach (KeyValuePair<PlayerModifiers, int> mod in i.Modifiers)
                        {
                            ApplyPlayerModifiers(mod);
                        }
                        break;
                    case Necklace i:
                        foreach (KeyValuePair<PlayerModifiers, int> mod in i.Modifiers)
                        {
                            ApplyPlayerModifiers(mod);
                        }
                        break;
                    case Armor i:
                        foreach (KeyValuePair<DamageModifiers, int> mod in i.ArmorModifiers)
                        {
                            ApplyArmorModifiers(mod);
                        }
                        break;
                    case Weapon i:
                        foreach (KeyValuePair<DamageModifiers, int> mod in i.WeaponDamageModifers)
                        {
                            ApplyWeaponModifiers(mod);
                        }
                        break;
                }
            }

            if (replacedItem != null)
            {
                switch (replacedItem)
                {
                    case Ring i:
                        foreach (KeyValuePair<PlayerModifiers, int> mod in i.Modifiers)
                        {
                            RemovePlayerModifiers(mod);
                        }
                        break;
                    case Necklace i:
                        foreach (KeyValuePair<PlayerModifiers, int> mod in i.Modifiers)
                        {
                            RemovePlayerModifiers(mod);
                        }
                        break;
                    case Armor i:
                        foreach (KeyValuePair<DamageModifiers, int> mod in i.ArmorModifiers)
                        {
                            RemoveArmorModifiers(mod);
                        }
                        break;
                    case Weapon i:
                        foreach (KeyValuePair<DamageModifiers, int> mod in i.WeaponDamageModifers)
                        {
                            RemoveWeaponModifiers(mod);
                        }
                        break;
                }
            }

            UpdateAttackPower();
            UpdateArmorValue();
        }
        /// <summary>
        /// Applys modifiers to stats when a new item is added
        /// </summary>
        /// <param name="mod">Modification to apply</param>
        private void ApplyPlayerModifiers(KeyValuePair<PlayerModifiers, int> mod)
        {
            switch (mod.Key)
            {
                case PlayerModifiers.Health:
                    MaxHealth += mod.Value;
                    CurrentHealth += mod.Value;
                    break;
                case PlayerModifiers.Magic:
                    MaxMagic += mod.Value;
                    CurrentMagic += mod.Value;
                    break;
                case PlayerModifiers.Speed:
                    MaxSpeed += mod.Value;
                    CurrentSpeed += mod.Value;
                    break;
            }
        }
        /// <summary>
        /// Removes modifiers to stats when an item is removed
        /// </summary>
        /// <param name="mod">Modifications to remove</param>
        private void RemovePlayerModifiers(KeyValuePair<PlayerModifiers, int> mod)
        {
            switch (mod.Key)
            {
                case PlayerModifiers.Health:
                    MaxHealth -= mod.Value;
                    CurrentHealth -= mod.Value;
                    break;
                case PlayerModifiers.Magic:
                    MaxMagic -= mod.Value;
                    CurrentMagic -= mod.Value;
                    break;
                case PlayerModifiers.Speed:
                    MaxSpeed -= mod.Value;
                    CurrentSpeed -= mod.Value;
                    break;
            }
        }
        /// <summary>
        /// Adds armor modifications to the ArmorMods dictionary
        /// </summary>
        /// <param name="mod">Mod to apply</param>
        private void ApplyArmorModifiers(KeyValuePair<DamageModifiers, int> mod)
        {
            if (ArmorModifiers.ContainsKey(mod.Key))
            {
                ArmorModifiers[mod.Key] += mod.Value;
            }
            else
            {
                ArmorModifiers.Add(mod.Key, mod.Value);
            }
        }
        /// <summary>
        /// Removes armor modifiers from the armor mod dict
        /// </summary>
        /// <param name="mod">Mod to remove</param>
        private void RemoveArmorModifiers(KeyValuePair<DamageModifiers, int> mod)
        {
            if (ArmorModifiers.ContainsKey(mod.Key))
            {
                ArmorModifiers[mod.Key] -= mod.Value;
                // If the value is 0 after subtraction, remove modifier
                if (ArmorModifiers[mod.Key] == 0)
                {
                    ArmorModifiers.Remove(mod.Key);
                }
            }

        }
        /// <summary>
        /// Adds Attack modifications to the Attack dictionary
        /// </summary>
        /// <param name="mod">Mod to apply</param>
        private void ApplyWeaponModifiers(KeyValuePair<DamageModifiers, int> mod)
        {
            if (AttackModifiers.ContainsKey(mod.Key))
            {
                AttackModifiers[mod.Key] += mod.Value;
            }
            else
            {
                AttackModifiers.Add(mod.Key, mod.Value);
            }
        }
        /// <summary>
        /// Removes Attack modifiers from the Attack mod dict
        /// </summary>
        /// <param name="mod">Mod to remove</param>
        private void RemoveWeaponModifiers(KeyValuePair<DamageModifiers, int> mod)
        {
            if (AttackModifiers.ContainsKey(mod.Key))
            {
                AttackModifiers[mod.Key] -= mod.Value;
                // If the value is 0 after subtraction, remove modifier
                if (AttackModifiers[mod.Key] == 0)
                {
                    AttackModifiers.Remove(mod.Key);
                }
            }

        }
        /// <summary>
        /// Updates Armor to reflect new BaseArmor stats
        /// </summary>
        private void UpdateArmorValue()
        {
            Armor = 0;
            Armor += Inv.HeadArmor != null ? Inv.HeadArmor.BaseArmor : 0;
            Armor += Inv.ChestArmor != null ? Inv.ChestArmor.BaseArmor : 0;
            Armor += Inv.LegArmor != null ? Inv.LegArmor.BaseArmor : 0;
            Armor += Inv.FeetArmor != null ? Inv.FeetArmor.BaseArmor : 0;
        }

        private void UpdateAttackPower()
        {
            AttackPower = 0;
            AttackPower += Inv.PrimaryWeapon != null ? Inv.PrimaryWeapon.BaseDamage : 0;
            AttackPower += Inv.SecondaryWeapon != null ? Inv.SecondaryWeapon.BaseDamage : 0;
        }
        #endregion
        #region Combat
        /*
        public enum DamageType
        {
            Health,
            Magic
        }

        public enum HitPosition
        {
            Head = 0,
            Chest = 1,
            Legs = 2,
            Feet = 3,
            None = 4
        }

        public HitPosition GetHitPos(int i)
        {
            switch(i)
            {
                case 0: return HitPosition.Head;
                case 1: return HitPosition.Chest;
                case 2: return HitPosition.Legs;
                case 3: return HitPosition.Feet;
                default: return HitPosition.None;
            }
        }
        */
        //protected static readonly double[] BodypartHitChances = {/*Head Hit*/ 0.05,/*Chest Hit*/ .55,/*Leg Hit*/ .85,/*Feet Hit*/ 1.0 };

        /// <summary>
        /// Attacks an enemy target
        /// </summary>
        /// <param name="enemy">Enemy LGO to attack</param>
        /// <param name="dmg">The ammount of damage dealt to the enemy.</param>
        /// <returns>Did the attack hit or not</returns>
        public bool Attack(LivingGameObject enemy, out int dmg)
        {
            if (Program._rand.NextDouble() + noMissModifier >= enemy.GetDodgeChance())
            {
                /*
                HitPosition hitPos = HitPosition.Chest;
                double val = Program._rand.NextDouble();
                for (int i = 0; i < BodypartHitChances.Length; i++)
                {
                    if(val < BodypartHitChances[i])
                    {
                        hitPos = GetHitPos(i);
                        break;
                    }
                }
                */
                int totalDamage = 0;
                Dictionary<DamageModifiers, int> selfAttack = AttackModifiers;
                Dictionary<DamageModifiers, int> enemyDefense = enemy.ArmorModifiers;
                foreach (KeyValuePair<DamageModifiers, int> mod in selfAttack)
                {
                    if (enemyDefense.ContainsKey(mod.Key))
                    {
                        totalDamage += enemy.ReceiveModDamage(mod.Value, enemyDefense[mod.Key]);
                    }
                }

                totalDamage += enemy.ReceiveBaseDamage(totalDamage + AttackPower);
                noMissModifier = 0.0;
                dmg = totalDamage;
                return true;
            }
            else
            {
                noMissModifier += 0.05;
                dmg = 0;
                return false;
            }
        }
        /// <summary>
        /// Gets the default hit percentage of the current object.
        /// </summary>
        /// <returns>Chance that the object will dodge.</returns>
        public double GetDodgeChance()
        {
            double hitChance = 0.05;
            hitChance -= 0.02 * Armor;
            hitChance += 0.05 * CurrentSpeed;

            if (hitChance > .95)
            {
                return .95;
            }
            else if (hitChance < .05)
            {
                return .05;
            }
            else
            {
                return hitChance;
            }
        }

        protected int ReceiveBaseDamage(int dmgAmnt)
        {
            int damage = dmgAmnt - Armor;
            if(!(damage > 0))
            {
                damage = 1;
            }

            CurrentHealth -= damage;

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Fainted = true;
                DidFaint(this);
            }

            return damage;
        }

        protected int ReceiveModDamage(int enemyAttack, int defenseAmmount)
        {
            return (enemyAttack - defenseAmmount > 0) ? enemyAttack - defenseAmmount : 0;
        }
        #endregion

        /// <summary>
        /// Returns health, magic, and stamina values to their max.
        /// </summary>
        public void ResetLivingObject()
        {
            CurrentHealth = MaxHealth;
            CurrentMagic = MaxMagic;
            CurrentSpeed = MaxSpeed;
        }

        
    }
}
