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
using ModdingUtils.AIMinion.Extensions;
using ModsPlus;

namespace CardsPlusPlugin.Cards
{
    public class QuickReflexesCard : CustomEffectCard<QuickReflexesEffect>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "QuickReflexes",
            Description = "Automatically block bullets",
            ModName     = "Cards+",
            Art         = Assets.QuickReflexesArt,
            Rarity      = CardInfo.Rarity.Rare,
            Theme       = CardThemeColor.CardThemeColorType.DefensiveBlue,
            Stats = new []
            {
                new CardInfoStat()
                {
                    positive = false,
                    amount = "2s",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned,
                    stat = "Block cooldown"
                }
            }
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            cardInfo.allowMultiple = false;
            block.cdAdd = 2f;
        }
    }

    public class QuickReflexesEffect : CardEffect
    {
        // blank mono which just marks the player as being able to autoblock
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
                if (healthHandler.GetComponentInChildren<QuickReflexesEffect>() != null && block.counter >= block.Cooldown())
                {
                    wasBlocked = true;
                    var data = healthHandler.GetComponent<CharacterData>();
                    if (data.view.IsMine && !data.GetAdditionalData().isAIMinion) { block.TryBlock(); }
                }
            }
        }
    }
}
