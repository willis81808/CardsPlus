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
    public class BurnoutCard : CustomEffectCard<CyberpunkHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Burnout",
            Description = $"<color=\"purple\">Quickhack</color>\n" +
                          $"<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.BURNOUT]} RAM</color>\n" +
                          $"Damage target. Does more damage the lower their health is",
            ModName     = "Cards+",
            Art         = Assets.BurnoutArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.DestructiveRed,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.BURNOUT));
            CyberpunkClass.BuffDrawChance(3);
        }

        protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.BURNOUT);
            CyberpunkClass.BuffDrawChance(-3);
        }

        public static void DoQuickHack(Player target, Player source)
        {
            if (target.data.dead) return;
            var healthPercentage = target.data.health / target.data.maxHealth;
            var damage = Vector2.one * ((target.data.maxHealth * 0.2f) / healthPercentage);
            NetworkingManager.RPC(typeof(BurnoutCard), nameof(RPC_ApplyDamage), target.playerID, damage);
        }

        [UnboundRPC]
        private static void RPC_ApplyDamage(int playerId, Vector2 damage)
        {
            var target = PlayerManager.instance.GetPlayerWithID(playerId);
            target.data.healthHandler.TakeDamage(damage, target.transform.position, ignoreBlock: true);
        }
    }
}
