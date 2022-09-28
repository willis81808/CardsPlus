using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;

namespace CardsPlusPlugin.Utils
{
    public static class ExtensionMethods
    {
        public static void RemovePlayerDiedAction(this PlayerManager pm, Action<Player, int> listener)
        {
            var action = (Action<Player, int>) pm.GetFieldValue("PlayerDiedAction");
            action -= listener;
        }

        public static Player GetPlayerWithID(this PlayerManager pm, int playerId)
        {
            return (Player) pm.InvokeMethod("GetPlayerWithID", playerId);
        }
    }
}
