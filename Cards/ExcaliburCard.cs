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
    public class ExcaliburCard : CustomCard
    {
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var spawner = player.GetComponent<SwordSpawner>();

            if (spawner != null)
            {
                spawner.Upgrade();
            }
            else
            {
                spawner = player.gameObject.AddComponent<SwordSpawner>();
            }

            //spawner.target = PlayerManager.instance.GetOtherPlayer(player).transform;
            spawner.swordPrefab = Assets.SwordPrefab;

            block.BlockAction += (type) =>
            {
                spawner.SpawnSword();
                spawner.SpawnSword();
                spawner.SpawnSword();
            };
        }

        public override void OnRemoveCard()
        {
            //throw new NotImplementedException();
        }

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            block.InvokeMethod("ResetStats");
            block.cdMultiplier = 1.25f;
        }

        public override string GetModName()
        {
            return "Cards+";
        }

        protected override string GetTitle()
        {
            return "Excalibur";
        }

        protected override string GetDescription()
        {
            return "The sword of kings!";
        }

        protected override GameObject GetCardArt()
        {
            return Assets.ExcaliburArt;
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
                    stat = "Block cooldown",
                    amount = "+25%",
                    simepleAmount = CardInfoStat.SimpleAmount.slightlyLower
                }
            };
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.FirepowerYellow;
        }
    }
    
    public class SwordBehaviour : MonoBehaviour
    {
        public float tumbleSpeed = 200f;

        public float minDistance = 1f;

        public Transform target;

        public GameObject particleParent;

        public GameObject destroyParticles;

        private float flySpeed = 1f;

        private float? trackStartTime = null;

        private Rigidbody2D rb;

        private bool canDamage = true;

        public int damage = 75;

        private void Awake()
        {
            particleParent = transform.GetChild(0).gameObject;

            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        void Update()
        {
            particleParent.SetActive(target != null);

            if (target == null)
            {
                Tumble();
            }
            else
            {
                Attack();
            }
        }

        void Tumble()
        {
            transform.Rotate(transform.forward, tumbleSpeed * Time.deltaTime);
            transform.Rotate(Vector3.forward, tumbleSpeed * Time.deltaTime);
        }

        void Attack()
        {
            if (!trackStartTime.HasValue)
            {
                trackStartTime = Time.time;
            }
            else if (Time.time - trackStartTime < 0.5f)
            {
                var distanceFactor = Vector3.Distance(transform.position, target.position) + 1;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position), tumbleSpeed / distanceFactor * Time.deltaTime);
            }
            else if (Time.time - trackStartTime > 3)
            {
                Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
                Destroy(gameObject);
            }

            transform.position += transform.forward * flySpeed * Time.deltaTime;
            flySpeed = Mathf.Clamp(flySpeed * 1.2f, 0, 50f);

            if (canDamage && Vector3.Distance(transform.position, target.position) < minDistance)
            {
                canDamage = false;
                Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
                DamageTarget(target, damage);
                Destroy(gameObject, 0.1f);
            }

            if (Time.time - trackStartTime > 0.25f && canDamage && Physics2D.OverlapCircle(transform.position, 0.2f) != null)
            {
                canDamage = false;
                Instantiate(destroyParticles, transform.position, destroyParticles.transform.rotation);
                Destroy(gameObject, 0.1f);
            }
        }

        private void DamageTarget(Transform damageTarget, int damage)
        {
            var healthHandler = damageTarget.GetComponentInChildren<HealthHandler>();
            healthHandler.CallTakeDamage(damage * (damageTarget.position - transform.position), transform.position);

            //Sonigon.SoundManager.Instance.Play(PlayerManager.instance.players[0].data.healthHandler.soundDie, transform);

            var velocity = transform.forward * flySpeed;
            DynamicParticles.instance.PlayBulletHit(damage, transform, new HitInfo()
            {
                collider = null,
                normal = velocity,
                point = (Vector2)(transform.position + velocity) * 0.4f,
                rigidbody = rb,
                transform = transform
            }, Color.red);
        }
    }
    
    public class SwordSpawner : MonoBehaviour
    {
        public GameObject swordPrefab;

        public float pointDistance = 2f;

        public float smoothDampScalar = 30f;

        public float delay = 2f;

        private int damage = 75;

        private int spawnMax = 6;

        private int range = 20;

        private float scaleDelta = 0;

        public Transform target {
            get
            {
                var results = Physics2D.OverlapCircleAll(transform.position, range)
                    .OrderBy(r => Vector3.Distance(transform.position, r.transform.position));

                foreach (var r in results)
                {
                    var player = r.GetComponent<Player>();
                    if (player != null && player.transform != transform)
                        return player.transform;
                }

                return null;
            }
        }

        private GameObject targetPointHolder;

        private List<SwordData> swords = new List<SwordData>();

        void Awake()
        {
            targetPointHolder = new GameObject("Point Holder");
            targetPointHolder.transform.SetParent(transform);
        }

        void Start()
        {
            StartInvoke(delay);
        }

        public void StartInvoke(float delay)
        {
            CancelInvoke();
            this.delay = delay;
            InvokeRepeating("Fire", delay, delay);
        }

        void Update()
        {
            if (swords.Count == 0) return;

            targetPointHolder.transform.Rotate(Vector3.forward, -100f * Time.deltaTime);
            targetPointHolder.transform.position = transform.position;

            foreach (var d in swords)
            {
                d.sword.transform.position = Vector3.SmoothDamp(d.sword.transform.position, d.point.transform.position, ref d.velocity, Time.deltaTime * smoothDampScalar);
            }
        }

        public void Upgrade()
        {
            // fire faster
            delay /= 2;
            StartInvoke(delay);

            // increase max sword count
            spawnMax *= 2;

            // increase target range
            pointDistance += 0.2f;

            // increase damage
            damage *= 2;

            // increase range
            range += 5;

            // increase sword size
            scaleDelta += 0.2f;
        }

        void Fire()
        {
            if (swords.Count == 0 || target == null || Vector3.Distance(transform.position, target.position) > pointDistance * 10) return;
            ShootSword();
        }
        
        public void SpawnSword()
        {
            if (swords.Count >= spawnMax)
                return;

            var data = new SwordData();
            data.sword = Instantiate(swordPrefab, transform.position, swordPrefab.transform.rotation);
            data.sword.transform.localScale += Vector3.one * scaleDelta;
            
            var behaviour = data.sword.GetComponent<SwordBehaviour>();
            behaviour.damage = damage;
            behaviour.minDistance += scaleDelta;

            data.point = new GameObject("Follow Point");
            data.point.transform.SetParent(targetPointHolder.transform);
            swords.Add(data);

            UpdateSwordCount();
        }
        
        public void ShootSword()
        {
            int randIndex = UnityEngine.Random.Range(0, swords.Count);
            var data = swords[randIndex];
            swords.RemoveAt(randIndex);

            Destroy(data.point);

            data.sword.GetComponent<SwordBehaviour>().target = target;

            UpdateSwordCount();
        }

        private void UpdateSwordCount()
        {
            float angle = 360f / swords.Count;
            foreach (var d in swords)
            {
                d.point.transform.position = targetPointHolder.transform.position + Vector3.up * pointDistance;
                targetPointHolder.transform.Rotate(Vector3.forward, angle);
            }
        }

        private class SwordData
        {
            public GameObject sword;
            public GameObject point;
            public Vector3 velocity;
        }
    }
}
