using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using ModsPlus;
using UnboundLib.Networking;
using System.Collections;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class RebootOpticsCard : CustomEffectCard<CyberpunkHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Reboot Optics",
            Description = $"<color=\"purple\">Quickhack</color>\n" +
                          $"<color=\"red\">{QuickhackMenuOption.Costs[QuickhackMenuOption.QuickhackType.REBOOT_OPTICS]} RAM</color>\n" +
                          $"Reboot your target's optical drivers, blurring their screen for 5 seconds",
            ModName     = "Cards+",
            Art         = null,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.MagicPink,
            OwnerOnly   = true
        };
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.gameObject.GetOrAddComponent<CyberCardEffect>();
            cardInfo.allowMultiple = false;
        }

        protected override void Added(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            Unbound.Instance.ExecuteAfterFrames(1, () => QuickhackMenu.AddQuickhack(QuickhackMenuOption.QuickhackType.REBOOT_OPTICS));
        }

        protected override void Removed(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            QuickhackMenu.RemoveQuickhack(QuickhackMenuOption.QuickhackType.REBOOT_OPTICS);
        }

        public static void DoQuickHack(Player target, Player source)
        {
            if (target.data.dead) return;
            NetworkingManager.RPC(typeof(RebootOpticsCard), nameof(RPC_ApplyBlur), target.playerID, 5f);
        }

        [UnboundRPC]
        private static void RPC_ApplyBlur(int playerId, float duration)
        {
            var target = PlayerManager.instance.GetPlayerWithID(playerId);
            if (!target.data.view.IsMine) return;
            Unbound.Instance.StartCoroutine(DoBlurOn(target, duration));
        }

        private static IEnumerator DoBlurOn(Player target, float duration)
        {
            var blurEffect = Instantiate(Assets.ScreenBlurEffect, Unbound.Instance.canvas.transform).Initialize(0.5f, target.transform);
            var startTime = Time.time;
            yield return new WaitUntil(() => Time.time - startTime >= duration || target.data.dead);
            blurEffect.Remove(2f);
        }
    }
}
