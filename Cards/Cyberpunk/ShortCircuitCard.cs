using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class ShortCircuitCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var handler = player.GetComponent<CyberpunkHandler>();
            if (!handler)
            {
                player.gameObject.AddComponent<CyberpunkHandler>();
            }

            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT));
        }

        public override void OnRemoveCard()
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT);
        }

        protected override string GetTitle() => "Short Circuit";
        protected override string GetDescription() => "<color=\"purple\">Quickhack</color>\nStun target";
        public override string GetModName() => "Cards+";
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.DefensiveBlue;
        protected override CardInfoStat[] GetStats() => null;
        protected override GameObject GetCardArt() => null;
    }
}
