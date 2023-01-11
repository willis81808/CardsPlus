using ModsPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Networking;
using UnityEngine;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class ShortCircuitCard : CustomEffectCard<CyberpunkHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Short Circuit",
            Description = $"<color=\"purple\">Quickhack</color>\n" +
                          $"<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT]} RAM</color>\n" +
                          $"Stun target for 1 seconds",
            ModName     = "Cards+",
            Art         = Assets.ShortCircuitArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.ColdBlue,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT));
            CyberpunkClass.BuffDrawChance(3);
        }

        protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT);
            CyberpunkClass.BuffDrawChance(-3);
        }

        public static void DoQuickHack(Player target, Player source)
        {
            NetworkingManager.RPC(typeof(ShortCircuitCard), nameof(RPC_ApplyStun), target.playerID, 1f);
        }

        [UnboundRPC]
        private static void RPC_ApplyStun(int playerId, float duration)
        {
            var player = PlayerManager.instance.players.Where(p => p.playerID == playerId).First();
            player.data.stunHandler.AddStun(duration);
        }
    }
}
