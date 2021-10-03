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
    public class TurtleCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            statModifiers.movementSpeed = 0.50f;
            var block = gameObject.AddComponent<Block>();
            block.InvokeMethod("ResetStats");
            block.cdMultiplier = 0.5f;

            cardInfo.allowMultiple = false;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //throw new NotImplementedException();
        }
        public override void OnRemoveCard()
        {
            //throw new NotImplementedException();
        }

        public override string GetModName()
        {
            return "Cards+";
        }

        protected override string GetTitle()
        {
            return "Turtle";
        }
        protected override string GetDescription()
        {
            return null;
        }
        protected override GameObject GetCardArt()
        {
            return Assets.TurtleArt;
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
                    stat = "Block cooldown",
                    amount = "-50%",
                    simepleAmount = CardInfoStat.SimpleAmount.aLotLower
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Movement Speed",
                    amount = "-50%",
                    simepleAmount = CardInfoStat.SimpleAmount.aLotLower
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DefensiveBlue;
        }
    }
}
