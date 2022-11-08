using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using ModsPlus;
using CardsPlusPlugin.Utils;

namespace CardsPlusPlugin.Cards
{
    public class TestTurretCard : CustomEffectCard<TestTurretEffect>
    {
        public override CardDetails Details => new CardDetails
        {
            Title = "Turret Builder"
        };
    }

    public class TestTurretEffect : CardEffect
    {
        bool hasSpawned = false;

        public override void OnShoot(GameObject projectile)
        {
            base.OnShoot(projectile);

            if (hasSpawned) return;
            hasSpawned = true;
            projectile.AddComponent<TestTurretSpawner>();
        }
    }

    public class TestTurretSpawner : RayHitEffect
    {
        private bool done;

        public override HasToReturn DoHitEffect(HitInfo hit)
        {
            if (done) return HasToReturn.canContinue;

            var turret = Instantiate(Assets.TestTurret, transform.position, Quaternion.identity);

            var healthBar = turret.AddComponent<CustomHealthBar>();

            var damagable = turret.GetComponentInChildren<Rigidbody2D>().gameObject.AddComponent<DamagableEvent>();
            damagable.currentHP = 60;
            damagable.maxHP = 60;
            
            damagable.damageEvent = new UnityEngine.Events.UnityEvent();
            damagable.damageEvent.AddListener(() =>
            {
                CardsPlus.LOGGER.LogInfo("Turret Damaged");
                healthBar.CurrentHealth = damagable.currentHP;
            });

            healthBar.SetValues(damagable.currentHP, damagable.maxHP);

            done = true;
            return HasToReturn.canContinue;
        }
    }
}
