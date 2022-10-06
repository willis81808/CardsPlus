using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using ModsPlus;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class RamUpgradeCard : CustomEffectCard<CyberpunkHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "RAM Upgrade",
            Description = "New hardware!",
            ModName     = "Cards+",
            Art         = null,
            Rarity      = CardInfo.Rarity.Common,
            Theme       = CardThemeColor.CardThemeColorType.TechWhite,
            OwnerOnly   = true,
            Stats = new []
            {
                new CardInfoStat()
                {
                    stat            = "RAM",
                    amount          = "+2",
                    simepleAmount   = CardInfoStat.SimpleAmount.Some,
                    positive        = true
                }
            }
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.gameObject.AddComponent<CyberCardEffect>();
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            RamMenu.CreateRam(2, false);
        }
    }
}
