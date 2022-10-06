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
using CardsPlusPlugin.Cards.Cyberpunk;
using HarmonyLib;
using Photon.Pun;
using UnboundLib.GameModes;
using System.Collections;
using BepInEx.Logging;
using UnboundLib.Utils.UI;
using BepInEx.Configuration;
using CardsPlusPlugin.Utils;

namespace CardsPlusPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willis.rounds.modsplus", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("root.classes.manager.reborn", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.7.2")]
    [BepInProcess("Rounds.exe")]
    public class CardsPlus : BaseUnityPlugin
    {
        public static ManualLogSource LOGGER { get => Instance.Logger; }
        private static CardsPlus Instance { get; set; }
        private const string ModId = "com.willis.rounds.cardsplus";
        private const string ModName = "Cards Plus";
        private const string CompatabilityModName = "CardsPlus";

        internal static ConfigEntry<bool> allowSelfTargeting;

        void Awake()
        {
            Instance = this;

            Assets.OnFinishedLoadingAssets += OnAssetsLoaded;

            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            GameModeManager.AddHook(GameModeHooks.HookGameEnd, ResetGame);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, ResetRound);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, ResetRound);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, SetupPoint);
        }

        private void OnAssetsLoaded()
        {
            LOGGER.LogInfo("ASSETS DONE LOADING, REGISTER CARDS");
            RegisterPrefabs();
            RegisterCards();
        }

        void Start()
        {
            allowSelfTargeting = Config.Bind(CompatabilityModName, "CardsPlus_SelfTargeting", false, "Allow self-targeting with abilities using the player selector");
            Unbound.RegisterMenu(ModName, null, SetupMenu, showInPauseMenu: true);
        }

        private void SetupMenu(GameObject menu)
        {
            MenuHandler.CreateToggle(allowSelfTargeting.Value, "Allow Self Targeting with Adware/Quickhacks", menu, val => allowSelfTargeting.Value = val, 30, true);
        }

        private void RegisterPrefabs()
        {
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SnakePrefab.name, Assets.SnakePrefab);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.FlameArea.name, Assets.FlameArea);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SmokeObject.name, Assets.SmokeObject);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.AdwareCanvas.name, Assets.AdwareCanvas);
        }

        private void RegisterCards()
        {
            CardRegistry.RegisterCard<HareCard>();
            CardRegistry.RegisterCard<TurtleCard>();
            CardRegistry.RegisterCard<TerminatorCard>();
            CardRegistry.RegisterCard<SlowPokeCard>();
            CardRegistry.RegisterCard<PhantomCard>();
            CardRegistry.RegisterCard<QuickReflexesCard>();
            CardRegistry.RegisterCard<LowGravityCard>();
            CardRegistry.RegisterCard<SnakeAttackCard>();
            CardRegistry.RegisterCard<ExcaliburCard>();
            CardRegistry.RegisterCard<HotPotato>();
            CardRegistry.RegisterCard<SmokeGrenade>();
            CardRegistry.RegisterCard<AdwareCard>();

            CardRegistry.RegisterCard<ContagionCard>();
            CardRegistry.RegisterCard<ShortCircuitCard>();
            CardRegistry.RegisterCard<BurnoutCard>();
            CardRegistry.RegisterCard<HamperCard>();
            CardRegistry.RegisterCard<RamUpgradeCard>();
            CardRegistry.RegisterCard<OverclockCard>();
            CardRegistry.RegisterCard<CyberPsychosisCard>();
        }

        IEnumerator SetupPoint(IGameModeHandler gm)
        {
            //RamMenu.Empty();
            AdwareHandler.ResetCooldown();
            yield break;
        }

        IEnumerator ResetGame(IGameModeHandler gm)
        {
            DestroyAll<PhantomEffect>();
            DestroyAll<QuickReflexesEffect>();
            DestroyAll<SnakeShooter>();
            DestroyAll<SwordSpawner>();
            DestroyAll<HotPotatoEffect>();
            DestroyAll<SmokeLauncher>();
            DestroyAll<AdwareHandler>();
            DestroyAll<CyberpunkHandler>();
            yield break;
        }

        IEnumerator ResetRound(IGameModeHandler gm)
        {
            foreach (var swordSpawner in FindObjectsOfType<SwordSpawner>())
            {
                swordSpawner.Clear();
            }

            foreach (var snake in FindObjectsOfType<SnakeFollow>())
            {
                PhotonNetwork.Destroy(snake.gameObject);
            }

            foreach (var adwareHandler in FindObjectsOfType<AdwarePopup>())
            {
                Destroy(adwareHandler.gameObject);
            }

            foreach (var smokeEffect in FindObjectsOfType<Smoke>())
            {
                smokeEffect.Remove(0f);
            }

            AdwareHandler.ClearAdwareSelectors();
            QuickhackMenu.ClearQuickhackSelectors();

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
