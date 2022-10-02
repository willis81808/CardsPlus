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
    public class ContagionCard : CustomEffectCard<CyberpunkHandler>
    {
        private const int RANGE = 5;

        public override CardDetails Details => new CardDetails
        {
            Title       = "Contagion",
            Description = $"<color=\"purple\">Quickhack</color>\n" +
                            $"<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.CONTAGION]} RAM</color>\n" +
                            $"Deal poison damage to target and all players near to them",
            ModName     = "Cards+",
            Art         = Assets.ContagionArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.PoisonGreen,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.CONTAGION));
        }

        protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.CONTAGION);
        }

        public static void DoQuickHack(Player target)
        {
            var playersInRange = PlayerManager.instance.players
                .Where(p => Vector3.Distance(p.transform.position, target.transform.position) < RANGE)
                .Select(p => p.playerID)
                .ToArray();

            var damage = Vector2.one * 50f / playersInRange.Count();

            NetworkingManager.RPC(typeof(ContagionCard), nameof(RPC_ApplyPoison), playersInRange, damage, 5f, 1f);
        }

        [UnboundRPC]
        private static void RPC_ApplyPoison(int[] playerIds, Vector2 damage, float time, float interval)
        {
            foreach (var player in PlayerManager.instance.players.Where(p => playerIds.Contains(p.playerID)))
            {
                player.data.healthHandler.TakeDamageOverTime(damage, player.transform.position, time, interval, Color.green);
            }
        }
    }
}
