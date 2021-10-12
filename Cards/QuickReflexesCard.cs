using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using HarmonyLib;
using UnboundLib.Cards;
using UnityEngine;

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
        private Player player;

        void Awake()
        {
            player = GetComponent<Player>();
        }
        void Start()
        {
            if (player == null)
            {
                Destroy(this);
            }
        }
        void Update()
        {
            foreach (BulletWrapper bulletWrapper in GetAllBullets())
            {
                RaycastHit2D raycastHit2D = Physics2D.Raycast(bulletWrapper.projectileMovement.transform.position, bulletWrapper.projectileMovement.velocity.normalized, bulletWrapper.velocity.magnitude * 5f * TimeHandler.deltaTime, LayerMask.GetMask("Default","Player","IgnorePlayer"));
                if (raycastHit2D.transform && (!bulletWrapper.projectileHit.ownPlayer || bulletWrapper.projectileHit.ownPlayer != player) && raycastHit2D.collider.GetComponentInParent<Player>() == player)
                {
                    player.data.block.TryBlock();
                }
            }
        }
        private List<BulletWrapper> GetAllBullets()
        {
            List<BulletWrapper> list = new List<BulletWrapper>();
            ProjectileHit[] array = UnityEngine.Object.FindObjectsOfType<ProjectileHit>();
            for (int i = 0; i < array.Length; i++)
            {
                BulletWrapper bulletWrapper = new BulletWrapper();
                bulletWrapper.projectileHit = array[i].GetComponent<ProjectileHit>();
                bulletWrapper.projectileMovement = array[i].GetComponent<MoveTransform>();
                bulletWrapper.damage = bulletWrapper.projectileHit.damage;
                bulletWrapper.velocity = bulletWrapper.projectileMovement.velocity;
                list.Add(bulletWrapper);
            }
            return list;
        }

        //[HarmonyPatch(typeof(Block), "RPCA_DoBlock")]
        //[HarmonyPostfix]
        //static void Block_PostFix(Block __instance, bool firstBlock, bool dontSetCD, BlockTrigger.BlockTriggerType triggerType, Vector3 useBlockPos, bool onlyBlockEffects)
        //{
        //    var phantomEffect = __instance.GetComponent<PhantomEffect>();
        //    if ((phantomEffect == null) || (triggerType != BlockTrigger.BlockTriggerType.Default)) return;
        //    phantomEffect.StartEffect();
        //}

        [HarmonyPatch(typeof(Block), "ResetStats")]
        [HarmonyPostfix]
        static void ResetStats_PostFix(Block __instance)
        {
            var quickReflexesEffect = __instance.GetComponent<QuickReflexesEffect>();
            if (quickReflexesEffect != null)
            {
                Destroy(quickReflexesEffect);
            }
        }
    }
}
