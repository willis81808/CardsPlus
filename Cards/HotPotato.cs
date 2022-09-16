using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards
{
    public class HotPotato : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.FlameArea.name, Assets.FlameArea);
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            player.gameObject.AddComponent<HotPotatoEffect>();
        }

        public override string GetModName() => "Cards+";
        protected override string GetTitle() => "Hot Potato";
        protected override string GetDescription() => "Leave a trail of burning fire in your wake";
        protected override GameObject GetCardArt() => Assets.HotPotatoArt;
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.DestructiveRed;
        protected override CardInfoStat[] GetStats() => null;
    }

    public class HotPotatoEffect : MonoBehaviour
    {
        private float spawnDelay = 0.1f;
        private float lifetime = 0.8f;
        private Player player;

        void Awake()
        {
            player = GetComponent<Player>();
        }

        void Start()
        {
            InvokeRepeating(nameof(SpawnFlame), spawnDelay, spawnDelay);
        }

        private void SpawnFlame()
        {
            var flameArea = Instantiate(Assets.FlameArea, player.transform.position, Quaternion.identity);
            flameArea.GetComponent<HotPotatoFlame>().Init(player);
            Destroy(flameArea, lifetime);
        }
    }

    public class HotPotatoFlame : MonoBehaviour
    {
        public static readonly int RANGE = 1;
        private static HashSet<Player> effectedPlayers = new HashSet<Player>();

        private Player owner;
        private float damage = 1;

        public void Init(Player owner)
        {
            this.owner = owner;
        }

        void FixedUpdate()
        {
            foreach (var obj in Physics2D.OverlapCircleAll(transform.position, RANGE))
            {
                var player = obj.GetComponent<Player>();
                if (player && player != owner && !effectedPlayers.Contains(player))
                {
                    effectedPlayers.Add(player);

                    var finalDamage = damage + (0.25f * owner.data.weaponHandler.gun.damage);
                    var damageDir = Vector3.Normalize(player.transform.position - transform.position) * finalDamage;
                    player.data.healthHandler.CallTakeDamage(damageDir, player.transform.position, null, owner);
                }
            }
            effectedPlayers.Clear();
        }
    }
}
