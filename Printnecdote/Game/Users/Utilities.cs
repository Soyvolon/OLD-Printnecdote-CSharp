using System;
using System.Collections.Generic;
using System.Text;

namespace Printnecdote.Game.Users
{
    public static class Utilities
    {
        /// <summary>
        /// Codes for different types of values to return from User methods
        /// </summary>
        public enum ReturnCodes
        {
            DefaultSuccess = 1,
            DefaultError = -1,
            InvalidInventorySlot = -2,
            InventoryFull = -3
        }

    }
}
