using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Pun;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards
{
    public class FragmentCard : CustomCard
    {
        ObjectsToSpawn[] reflectionBullets;
        BulletDuplicator reflectObject;
        
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            reflectObject = new GameObject("Reflect Object").AddComponent<BulletDuplicator>();
            DontDestroyOnLoad(reflectObject.gameObject);
            reflectionBullets = new ObjectsToSpawn[]
            {
                new ObjectsToSpawn()
                {
                    effect = reflectObject.gameObject,
                    spawnAsChild = false,
                    scaleFromDamage = 1,
                    scaleStackM = 1,
                    scaleStacks = true,
                    numberOfSpawns = 1,
                    spawnOn = ObjectsToSpawn.SpawnOn.notPlayer,
                    direction = ObjectsToSpawn.Direction.normal
                }
            };

            //gun.reflects = 1;
            gun.isProjectileGun = true;
            gun.objectsToSpawn = reflectionBullets;
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            reflectObject.gun = gun;
        }
        public override void OnRemoveCard()
        {
            //ModLoader.BuildInfoPopup("Removed Test Card");
        }

        protected override string GetTitle()
        {
            return "Fragment";
        }
        protected override string GetDescription()
        {
            return "Projectiles split into more projectiles on bounce";
        }
        protected override GameObject GetCardArt()
        {
            return null;
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                //new CardInfoStat()
                //{
                //    positive = true,
                //    stat = "Bullet bounces",
                //    amount = "+1",
                //    simepleAmount = CardInfoStat.SimpleAmount.aLittleBitOf
                //}
            };
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }
        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.DestructiveRed;
        }
    }

    public class BulletDuplicator : MonoBehaviour
    {
        internal static List<HitData> hits = new List<HitData>();

        private int depth = 0;
        private int maxDepth = 1;
        public bool initialized = false;
        public Gun gun;

        internal class HitData
        {
            public int depth, maxDepth;
            public BulletTracker bullet;

            public HitData(int depth, int maxDepth, BulletTracker bullet)
            {
                this.depth = depth;
                this.maxDepth = maxDepth;
                this.bullet = bullet;
            }
        }
        
        void Awake()
        {
            if (!initialized)
            {
                initialized = true;
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                var angle = 360f / i;
                InitBullet(gun.projectiles[0].objectToSpawn, angle);
            }

            var data = (from d in hits
                        where Vector2.Distance(transform.position, d.bullet.transform.position) < 1f
                        orderby d.depth descending
                        select d).FirstOrDefault();

            if (data != null)
            {
                depth = data.depth;
                maxDepth = data.maxDepth;
                hits.Remove(data);
            }
            else if (depth != 0)
            {
                return;
            }

            if (depth > maxDepth)
            {
                if (data != null) Destroy(data.bullet.gameObject);
                Destroy(gameObject);
                return;
            }

            depth++;

            InitBullet(gun.projectiles[0].objectToSpawn, 20);
            InitBullet(gun.projectiles[0].objectToSpawn, -20);
            
            if (data != null) Destroy(data.bullet.gameObject);
            Destroy(gameObject);
        }

        private void InitBullet(GameObject prefab, float rotation)
        {
            prefab.transform.rotation = transform.rotation;
            prefab.transform.Rotate(Camera.main.transform.forward, rotation + 180);
            var copy = PhotonNetwork.Instantiate(prefab.name, transform.position, prefab.transform.rotation);
            copy.transform.Rotate(Camera.main.transform.forward, rotation);

            var data = new HitData(depth, maxDepth, copy.AddComponent<BulletTracker>());
            hits.Add(data.bullet.AddData(data));
            copy.GetComponent<PhotonView>().RPC("RPCA_Init_noAmmoUse", RpcTarget.All, new object[]
            {
                gun.holdable.holder.view.OwnerActorNr,
                1,
                gun.bulletDamageMultiplier * Mathf.Max(0.25f, Mathf.Min(1, 1 - ((float)depth/maxDepth))),
                0.5f
            });
        }
    }

    public class BulletTracker : MonoBehaviour
    {
        private BulletDuplicator.HitData data;
        internal BulletDuplicator.HitData AddData(BulletDuplicator.HitData data)
        {
            this.data = data;
            return this.data;
        }
        void OnEnable()
        {
            Destroy(gameObject, 5f);
        }
        void OnDisable()
        {
            BulletDuplicator.hits.Remove(data);
        }
    }
}
