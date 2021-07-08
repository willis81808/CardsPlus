using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Pun;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.Events;

namespace CardsPlusPlugin.Cards
{
    public class SnakeAttackCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            gun.objectsToSpawn = new ObjectsToSpawn[]
            {
                new ObjectsToSpawn()
                {
                    effect = Assets.SnakeSpawner,
                    spawnAsChild = false,
                    scaleFromDamage = 1,
                    scaleStackM = 1,
                    scaleStacks = false,
                    numberOfSpawns = 1,
                    spawnOn = ObjectsToSpawn.SpawnOn.all,
                    direction = ObjectsToSpawn.Direction.normal
                }
            };
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats) { }

        public override void OnRemoveCard() { }

        protected override string GetTitle()
        {
            return "Snake Attack";
        }

        protected override string GetDescription()
        {
            return "I'm tired of all these goddamn snakes on this goddamn plane!";
        }

        protected override CardInfoStat[] GetStats()
        {
            return null;
        }

        protected override CardInfo.Rarity GetRarity()
        {
            return CardInfo.Rarity.Common;
        }

        protected override GameObject GetCardArt()
        {
            return null;
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.EvilPurple;
        }
    }

    public class SnakeSpawner : MonoBehaviour
    {
        private static bool Initialized = false;

        void Awake()
        {
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SnakePrefab.name, Assets.SnakePrefab);
        }

        void Start()
        {
            if (!Initialized)
            {
                Initialized = true;
                return;
            }

            Destroy(gameObject, 1f);

            if (!PhotonNetwork.OfflineMode && !PhotonNetwork.IsMasterClient) return;

            this.ExecuteAfterSeconds(0.1f, () =>
            {
                var result = PhotonNetwork.Instantiate(Assets.SnakePrefab.name, transform.position, transform.rotation).GetComponent<SnakeFollow>();
                result.SetDamageScale(transform.lossyScale.x);
            });
        }
    }
    
    [RequireComponent(typeof(Rigidbody2D), typeof(LineRenderer))]
    public class SnakeFollow : MonoBehaviour
    {
        private Rigidbody2D rb;
        private PhotonView view;
        private LineRenderer line;

        private Vector2 velocity, velRef;

        private float wanderStrength = 12f;
        private float followStrength = 0.75f;
        private float smoothStrength = 0.35f;
        private float maxSpeed = 20;
        private float damageScale = 1f;
        
        public Transform target;

        private static readonly int DamageLayerMask = LayerMask.GetMask("Player");

        static SnakeFollow()
        {
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, DeleteAllSnakes);
        }

        private static IEnumerator DeleteAllSnakes(IGameModeHandler gm)
        {
            var snakes = FindObjectsOfType<SnakeFollow>();
            for (int i = 0; i < snakes.Length; i++)
            {
                PhotonNetwork.Destroy(snakes[i].gameObject);
            }
            yield break;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            view = GetComponent<PhotonView>();
            line = GetComponent<LineRenderer>();
        }
        
        private void Start()
        {
            // prevent collisions with all Background objects
            var backgroundObjects = GameObject.FindObjectsOfType<Collider2D>().Where(c => c.gameObject.layer == LayerMask.NameToLayer("BackgroundObject"));
            foreach (var c in GetComponentsInChildren<Collider2D>())
            {
                c.isTrigger = false;

                foreach (var c2 in backgroundObjects)
                {
                    Physics2D.IgnoreCollision(c, c2);
                }
            }

            // reset snake scale
            transform.localScale = Vector3.one;

            // destroy self when damaged too much
            var damagable = gameObject.AddComponent<DamagableEvent>();
            damagable.maxHP = damagable.currentHP = 1;
            damagable.deathEvent = new UnityEvent();
            damagable.damageEvent = new UnityEvent();
            damagable.deathEvent.AddListener(() =>
            {
                Sonigon.SoundManager.Instance.Play(PlayerManager.instance.players[0].data.healthHandler.soundDie, transform);

                DynamicParticles.instance.PlayBulletHit(20 * damageScale, transform, new HitInfo()
                {
                    collider = null,
                    normal = -rb.velocity.normalized,
                    point = (Vector2)transform.position + rb.velocity.normalized * 0.4f,
                    rigidbody = rb,
                    transform = transform
                }, Color.green);

                PhotonNetwork.Destroy(gameObject);
            });
        }

        private void Update()
        {
            // follow nearest player
            target = (from p in PlayerManager.instance.players
                      orderby Vector3.Distance(p.transform.position, transform.position)
                      select p.transform).FirstOrDefault();

            // check if should deal damage
            var nearby = Physics2D.OverlapCircleAll((Vector2)transform.position + rb.velocity.normalized * 0.4f, 1f, DamageLayerMask);
            foreach (var t in nearby)
            {
                if (t.transform.CompareTag("Player"))
                {
                    DamageTarget(t.transform);
                }
            }

            if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
            {
                RandomizeMovement();
                FollowTarget();
                RotateHead();
            }

            // clamp max speed
            velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

            // apply new velocity
            rb.velocity = Vector2.SmoothDamp(rb.velocity, velocity, ref velRef, smoothStrength);
        }

        public void SetDamageScale(float scale)
        {
            view.RPC(nameof(RPC_SetDamageScale), RpcTarget.All, scale);
        }

        [PunRPC]
        private void RPC_SetDamageScale(float scale)
        {
            damageScale = scale;
        }

        private void RandomizeMovement()
        {
            velocity += Mathf.PerlinNoise(transform.position.x, transform.position.y) * UnityEngine.Random.insideUnitCircle.normalized * wanderStrength;
        }

        private void FollowTarget()
        {
            if (target != null)
            {
                velocity += (Vector2)(target.position - transform.position).normalized * followStrength;
            }
        }

        private void RotateHead()
        {
            if (transform.childCount == 0) return;

            var head = transform.GetChild(0);
            head.transform.up = rb.velocity;
        }
        
        private void DamageTarget(Transform damageTarget)
        {
            var healthHandler = damageTarget.GetComponentInChildren<HealthHandler>();
            healthHandler.CallTakeDamage(20 * damageScale * (damageTarget.position - transform.position), transform.position);

            Sonigon.SoundManager.Instance.Play(PlayerManager.instance.players[0].data.healthHandler.soundDie, transform);

            DynamicParticles.instance.PlayBulletHit(20 * damageScale, transform, new HitInfo()
            {
                collider = null,
                normal = rb.velocity.normalized,
                point = (Vector2)transform.position + rb.velocity.normalized * 0.4f,
                rigidbody = rb,
                transform = transform
            }, Color.red);

            PhotonNetwork.Destroy(gameObject);
        }
    }
}
