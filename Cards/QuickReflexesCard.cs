using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using HarmonyLib;
using UnboundLib.Cards;
using UnityEngine;
using Photon.Pun;

namespace CardsPlusPlugin.Cards
{
    public class QuickReflexesCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            block.InvokeMethod("ResetStats");
            block.cdAdd = 2f;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.AddComponent<QuickReflexesEffect>();
        }
        public override void OnRemoveCard() { }

        public override string GetModName()
        {
            return "Cards+";
        }

        protected override string GetTitle()
        {
            return "Quick Reflexes";
        }
        protected override string GetDescription()
        {
            return "Automatically block bullets";
        }
        protected override GameObject GetCardArt()
        {
            return Assets.QuickReflexesArt;
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Rare;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = false,
                    amount = "2s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Block cooldown"
                }
            };
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DefensiveBlue;
        }
    }

    public class QuickReflexesEffect : MonoBehaviour
    {
        // blank mono which just marks the player as being able to autoblock
    }
    // necessary patches

    // these patches were not running previously when put inside QuickReflexesEffect
    // organizing them this way causes them to run properly

    [HarmonyPatch(typeof(Block), "ResetStats")]
    class BlockPatchResetStats
    {
        static void PostFix(Block __instance)
        {
            var quickReflexesEffect = __instance.GetComponent<QuickReflexesEffect>();
            if (quickReflexesEffect != null)
            {
                GameObject.Destroy(quickReflexesEffect);
            }
        }
    }
    [HarmonyPatch(typeof(ProjectileHit), "RPCA_DoHit")]
    [HarmonyPriority(Priority.First)]
    class ProjectileHitPatchRPCA_DoHit
    {
        private static void Prefix(ProjectileHit __instance, Vector2 hitPoint, Vector2 hitNormal, Vector2 vel, int viewID, int colliderID, ref bool wasBlocked)
        {
            // prefix to allow autoblocking

            HitInfo hitInfo = new HitInfo();
            hitInfo.point = hitPoint;
            hitInfo.normal = hitNormal;
            hitInfo.collider = null;
            if (viewID != -1)
            {
                PhotonView photonView = PhotonNetwork.GetPhotonView(viewID);
                hitInfo.collider = photonView.GetComponentInChildren<Collider2D>();
                hitInfo.transform = photonView.transform;
            }
            else if (colliderID != -1)
            {
                hitInfo.collider = MapManager.instance.currentMap.Map.GetComponentsInChildren<Collider2D>()[colliderID];
                hitInfo.transform = hitInfo.collider.transform;
            }
            HealthHandler healthHandler = null;
            if (hitInfo.transform)
            {
                healthHandler = hitInfo.transform.GetComponent<HealthHandler>();
            }
            if (healthHandler && healthHandler.GetComponent<CharacterData>() && healthHandler.GetComponent<Block>())
            {
                Block block = healthHandler.GetComponent<Block>();
                if (healthHandler.GetComponent<QuickReflexesEffect>() != null && block.counter >= block.Cooldown())
                {
                    wasBlocked = true;
                    if (healthHandler.GetComponent<CharacterData>().view.IsMine) { block.TryBlock(); }
                }
            }
        }
    }
}
