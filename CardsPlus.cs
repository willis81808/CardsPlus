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
    [BepInPlugin(ModId, ModName, "1.6.0")]
    [BepInProcess("Rounds.exe")]
    public class CardsPlus : BaseUnityPlugin
    {
        private static CardsPlus Instance { get; set; }
        private const string ModId = "com.willis.rounds.cardsplus";
        private const string ModName = "Cards Plus";
        private const string CompatabilityModName = "CardsPlus";

        internal static ConfigEntry<bool> allowSelfTargeting;

        void Awake()
        {
            Instance = this;

            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            GameModeManager.AddHook(GameModeHooks.HookGameEnd, ResetGame);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, ResetRound);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, ResetRound);
            GameModeManager.AddHook(GameModeHooks.HookPointStart, SetupPoint);

            RegisterPrefabs();
        }

        void Start()
        {
            allowSelfTargeting = Config.Bind(CompatabilityModName, "CardsPlus_SelfTargeting", false, "Allow self-targeting with abilities using the player selector");
            Unbound.RegisterMenu(ModName, null, SetupMenu, showInPauseMenu: true);

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
            CustomCard.BuildCard<AdwareCard>();
            CustomCard.BuildCard<ContagionCard>();
            CustomCard.BuildCard<ShortCircuitCard>();
            CustomCard.BuildCard<BurnoutCard>();
            CustomCard.BuildCard<HamperCard>();
        }

        public static ManualLogSource GetLogger()
        {
            return Instance.Logger;
        }

        private void SetupMenu(GameObject menu)
        {
            MenuHandler.CreateToggle(allowSelfTargeting.Value, "Allow Self Targeting with Adware/Quickhacks", menu, val => allowSelfTargeting.Value = val, 30, true);
        }

        private void RegisterPrefabs()
        {
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SnakePrefab.name, Assets.SnakePrefab);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.SmokeObject.name, Assets.SmokeObject);
            PhotonNetwork.PrefabPool.RegisterPrefab(Assets.AdwareCanvas.name, Assets.AdwareCanvas);
        }

        IEnumerator SetupPoint(IGameModeHandler gm)
        {
            RamMenu.Empty();
            yield break;
        }

        IEnumerator ResetGame(IGameModeHandler gm)
        {
            DestroyAll<PhantomEffect>();
            DestroyAll<QuickReflexesEffect>();
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
                smokeEffect.Remove();
            }

            PlayerSelector.Clear();

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
