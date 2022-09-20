using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx;
using UnboundLib;
using UnboundLib.Cards;
using CardsPlusPlugin.Cards;
using HarmonyLib;
using Photon.Pun;
using UnboundLib.GameModes;
using System.Collections;

namespace CardsPlusPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.4.0")]
    [BepInProcess("Rounds.exe")]
    public class CardsPlus : BaseUnityPlugin
    {
        private const string ModId = "com.willis.rounds.cardsplus";
        private const string ModName = "Cards Plus";
        public const string TEST = "";
        
        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            GameModeManager.AddHook(GameModeHooks.HookGameEnd, ResetEffects);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, ResetRound);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, ResetRound);

            RegisterPrefabs();
        }

        void Start()
        {
            /*
            CustomCard.BuildCard<HareCard>();
            CustomCard.BuildCard<TurtleCard>();
            CustomCard.BuildCard<TerminatorCard>();
            CustomCard.BuildCard<SlowPokeCard>();
            CustomCard.BuildCard<PhantomCard>();
            CustomCard.BuildCard<QuickReflexesCard>();
            CustomCard.BuildCard<LowGravityCard>();
            CustomCard.BuildCard<SnakeAttackCard>();
            CustomCard.BuildCard<ExcaliburCard>();
            CustomCard.BuildCard<HotPotato>();
            CustomCard.BuildCard<SmokeGrenade>();
            */
            CustomCard.BuildCard<AdwareCard>();
        }

        private void RegisterPrefabs()
        {
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SnakePrefab.name, Assets.SnakePrefab);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SmokeObject.name, Assets.SmokeObject);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.AdwareCanvas.name, Assets.AdwareCanvas);
        }

        IEnumerator ResetEffects(IGameModeHandler gm)
        {
            DestroyAll<PhantomEffect>();
            DestroyAll<QuickReflexesEffect>();
            DestroyAll<SwordSpawner>();
            DestroyAll<HotPotatoEffect>();
            DestroyAll<SmokeLauncher>();
            yield break;
        }

        IEnumerator ResetRound(IGameModeHandler gm)
        {
            foreach (var player in PlayerManager.instance.players)
            {
                var swordSpawner = player.GetComponent<SwordSpawner>();
                if (swordSpawner != null) swordSpawner.Clear();
            }

            var snakes = FindObjectsOfType<SnakeFollow>();
            for (int i = 0; i < snakes.Length; i++)
            {
                PhotonNetwork.Destroy(snakes[i].gameObject);
            }

            foreach (var adwareHandler in FindObjectsOfType<AdwarePopup>())
            {
                Destroy(adwareHandler.gameObject);
            }

            yield break;
        }

        void DestroyAll<T>() where T : UnityEngine.Object
        {
            var objects = GameObject.FindObjectsOfType<T>();
            for (int i = objects.Length - 1; i >= 0; i--)
            {
                Destroy(objects[i]);
            }
        }
    }
}
