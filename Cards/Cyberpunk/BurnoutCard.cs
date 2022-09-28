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
    public class BurnoutCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (!player.data.view.IsMine) return;

            var handler = player.GetComponent<CyberpunkHandler>();
            if (!handler)
            {
                player.gameObject.AddComponent<CyberpunkHandler>();
            }

            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.BURNOUT));
        }

        public override void OnRemoveCard()
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.BURNOUT);
        }

        protected override string GetTitle() => "Burnout";
        protected override string GetDescription() => $"<color=\"purple\">Quickhack</color>\n<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.BURNOUT]} RAM</color>\nDamage target. Does more damage the lower their health is";
        public override string GetModName() => "Cards+";
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.DestructiveRed;
        protected override CardInfoStat[] GetStats() => null;
        protected override GameObject GetCardArt() => Assets.BurnoutArt;

        public static void DoQuickHack(Player target)
        {
            if (target.data.dead) return;
            var healthPercentage = target.data.health / target.data.maxHealth;
            var damage = Vector2.one * ((target.data.maxHealth * 0.2f) / healthPercentage);
            NetworkingManager.RPC(typeof(BurnoutCard), nameof(RPC_ApplyDamage), target.playerID, damage);
        }

        [UnboundRPC]
        private static void RPC_ApplyDamage(int playerId, Vector2 damage)
        {
            var target = PlayerManager.instance.players.Where(p => p.playerID == playerId).First();
            target.data.healthHandler.TakeDamage(damage, target.transform.position, ignoreBlock: true);
        }
    }
}
