using Printnecdote.Game.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Items;
using Items.Modifiers;
using Newtonsoft.Json;

namespace Printnecdote.Game
{
    /// <summary>
    /// Holds the local cache for player information.
    /// </summary>
    public class Player : LivingGameObject
    {
        
        /// <summary>
        /// Id Ulong - Matches Discord User ID
        /// </summary>
        public ulong Id { get; private set; }
        public Player(ulong id, string newName) : base(newName)
        {
            Id = id;
        }
        public Player(IUser user, string newName) : this(user.Id, newName) { }
        public Player(ulong id) : this(id, "") { }
        public Player(IUser user) : this(user, "") { }
        
        [JsonConstructor]
        public Player(string name, Inventory inv, int maxHealth, int currentHealth, int maxMagic, int currentMagic, int maxSpeed, int currentSpeed, int armor, Dictionary<DamageModifiers, int> armorModifiers, int attackPower, Dictionary<DamageModifiers, int> attackModifiers, int level, bool fainted, double noMissModifier, ulong id)
            : base(name, inv, maxHealth, currentHealth, maxMagic, currentMagic, maxSpeed, currentSpeed, armor, armorModifiers, attackPower, attackModifiers, level, fainted, noMissModifier)
        {
            Id = id;
        }
    }
}
