using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsPlusPlugin.Utils;
using Photon.Pun;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.Events;

namespace CardsPlusPlugin.Cards
{
    public class SnakeAttackCard : CustomEffectCard<SnakeShooter>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Snake Attack",
            Description = "Snakes... Why did it have to be snakes??",
            ModName     = "Cards+",
            Art         = Assets.SnakeAttackArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.PoisonGreen,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }
    }

    public class SnakeShooter : CardEffect
    {
        public override void OnShoot(GameObject projectile)
        {
            var spawnedAttack = projectile.GetComponent<SpawnedAttack>();
            if (!spawnedAttack) return;
            projectile.AddComponent<SnakeSpawner>();
        }
    }

    public class SnakeSpawner : RayHitEffect
    {
        private bool done;

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (done || SnakeFollow.maxSnakeCount <= SnakeFollow.snakeCount || (!PhotonNetwork.OfflineMode && !PhotonNetwork.IsMasterClient))
                return HasToReturn.canContinue;

            if (hit.transform.GetComponentInParent<Player>() || hit.transform.GetComponentInParent<SnakeFollow>())
                return HasToReturn.canContinue;

            var result = PhotonNetwork.Instantiate(Assets.SnakePrefab.name, transform.position, Assets.SnakePrefab.transform.rotation).GetComponent<SnakeFollow>();
            result.SetDamageScale(transform.lossyScale.x);

            done = true;
            return HasToReturn.canContinue;
        }
    }
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class SnakeFollow : MonoBehaviour
    {
        internal static int snakeCount = 0;
        internal static readonly int maxSnakeCount = 10;

        private Rigidbody2D rb;
        private PhotonView view;

        private DamagableEvent damagable;
        private CustomHealthBar customHealthBar;

        private Vector2 velocity, velRef;

        private float wanderStrength = 12f;
        private float followStrength = 0.75f;
        private float smoothStrength = 0.35f;
        private float maxSpeed = 20;
        private float damageScale = 1f;
        private float maxHp = 75;
        
        public Transform target;

        private static readonly int DamageLayerMask = LayerMask.GetMask("Player");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            view = GetComponent<PhotonView>();

            damagable = gameObject.AddComponent<DamagableEvent>();
            customHealthBar = gameObject.AddComponent<CustomHealthBar>();

            rb.isKinematic = false;
            rb.mass = 2000f;
            rb.inertia = float.PositiveInfinity;

            snakeCount++;
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
            damagable.currentHP = maxHp;
            damagable.maxHP = maxHp;
            damagable.deathEvent = new UnityEvent();
            damagable.deathEvent.AddListener(OnDeath);
            damagable.damageEvent = new UnityEvent();
            damagable.damageEvent.AddListener(OnDamage);

            // setup health bar
            customHealthBar.SetValues(damagable.currentHP, damagable.maxHP);
        }

        private void OnDamage()
        {
            customHealthBar.CurrentHealth = damagable.currentHP;
        }

        private void OnDeath()
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

            Unbound.Instance.ExecuteAfterFrames(1, () => PhotonNetwork.Destroy(gameObject));
        }

        private void Update()
        {
            // follow nearest player
            target = PlayerManager.instance.GetClosestPlayer(transform.position)?.transform;

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
            }
            RotateHead();

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

            var head = transform.Find("Head");
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

        private void OnDestroy()
        {
            snakeCount--;
        }
    }
}
