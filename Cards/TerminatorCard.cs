using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards
{
    public class TerminatorCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            statModifiers.health = 1.5f;
            gun.ammo = 5;
            gun.projectileSpeed = 3.5f;
            gun.reloadTimeAdd = 0.25f;
            gun.knockback = 2.5f;
            gun.damage = 0.4f;
            gun.destroyBulletAfter = 0.2f;
            gun.numberOfProjectiles = 4;
            gun.spread = 0.5f;
            gun.multiplySpread = 1f;
            gun.evenSpread = 0f;
            gun.drag = 10;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //throw new NotImplementedException();
        }
        public override void OnRemoveCard()
        {
            throw new NotImplementedException();
        }

        public override string GetModName()
        {
            return "Cards+";
        }

        protected override string GetTitle()
        {
            return "Terminator";
        }
        protected override string GetDescription()
        {
            return "Get to the choppa!";
        }
        protected override GameObject GetCardArt()
        {
            return Assets.TerminatorArt;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Uncommon;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = true,
                    stat = "HP",
                    amount = "+50%",
                    simepleAmount = CardInfoStat.SimpleAmount.aLotOf
                },
                new CardInfoStat()
                {
                    positive = true,
                    amount = "+5",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "AMMO"
                },
                new CardInfoStat()
                {
                    positive = true,
                    amount = "+4",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Bullets"
                },
                new CardInfoStat()
                {
                    positive = false,
                    amount = "+0.25s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Reload time"
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "DMG",
                    amount = "-60%",
                    simepleAmount = CardInfoStat.SimpleAmount.lower
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
        }
    }
}
