﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsPlusPlugin.Cards;
using CardsPlusPlugin.Cards.Cyberpunk;
using Photon.Pun;
using UnityEngine;

namespace CardsPlusPlugin
{
    internal class Assets
    {
        private static readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("cardsplusart", typeof(CardsPlus).Assembly);

        /*
         * CARD ART
         */
        public static GameObject TurtleArt = Bundle.LoadAsset<GameObject>("C_TheTurtle");
        public static GameObject HareArt = Bundle.LoadAsset<GameObject>("C_TheHare");
        public static GameObject TerminatorArt = Bundle.LoadAsset<GameObject>("C_Terminator");
        public static GameObject SlowPokeArt = Bundle.LoadAsset<GameObject>("C_SlowPoke");
        public static GameObject PhantomArt = Bundle.LoadAsset<GameObject>("C_Phantom");
        public static GameObject QuickReflexesArt = Bundle.LoadAsset<GameObject>("C_QuickReflexes");
        public static GameObject LowGravityArt = Bundle.LoadAsset<GameObject>("C_LowGravity");
        public static GameObject SnakeAttackArt = Bundle.LoadAsset<GameObject>("C_SnakeAttack");
        public static GameObject ExcaliburArt = Bundle.LoadAsset<GameObject>("C_Excalibur");
        public static GameObject HotPotatoArt = Bundle.LoadAsset<GameObject>("C_HotPotato");
        public static GameObject SmokeGrenadeArt = Bundle.LoadAsset<GameObject>("C_SmokeGrenade");
        public static GameObject AdwareArt = Bundle.LoadAsset<GameObject>("C_Adware");
        public static GameObject ShortCircuitArt = Bundle.LoadAsset<GameObject>("C_ShortCircuit").AddComponent<CyberCardEffect>().gameObject;
        public static GameObject HamperArt = Bundle.LoadAsset<GameObject>("C_Hamper").AddComponent<CyberCardEffect>().gameObject;
        public static GameObject ContagionArt = Bundle.LoadAsset<GameObject>("C_Contagion").AddComponent<CyberCardEffect>().gameObject;
        public static GameObject BurnoutArt = Bundle.LoadAsset<GameObject>("C_Burnout").AddComponent<CyberCardEffect>().gameObject;

        /*
         * Frames
         */
        public static GameObject ElectricFrame = Bundle.LoadAsset<GameObject>("Short Circuit Frame");

        /*
         * CARD ASSETS
         */

        // Snake attack
        public static GameObject SnakePrefab = Bundle.LoadAsset<GameObject>("Snake").AddComponent<Cards.SnakeFollow>().gameObject.AddComponent<PhotonView>().gameObject.AddComponent<NetworkPhysicsObject>().gameObject;
        public static GameObject SnakeSpawner = new GameObject("Snake Spawner").AddComponent<Cards.SnakeSpawner>().gameObject;

        // Excalibur
        public static GameObject SwordPrefab = Bundle.LoadAsset<GameObject>("Sword").AddComponent<Cards.SwordBehaviour>().gameObject;
        public static GameObject SwordExplosion = Bundle.LoadAsset<GameObject>("ProtonExplosionYellow");

        // Hot potato
        public static GameObject FlameArea = Bundle.LoadAsset<GameObject>("FlameArea").AddComponent<HotPotatoFlame>().gameObject;

        // Smoke Grenade
        public static GameObject SmokeObject = Bundle.LoadAsset<GameObject>("Smoke Object").AddComponent<PhotonView>().gameObject;
        public static GameObject SmokeEffect = Bundle.LoadAsset<GameObject>("Smoke Effect");

        // Adware
        public static GameObject PlayerSelector = Bundle.LoadAsset<GameObject>("Player Selector");
        public static GameObject PopupPrefab = Bundle.LoadAsset<GameObject>("Popup Window");
        public static GameObject HackedTargetEffect = Bundle.LoadAsset<GameObject>("HackedTargetEffect");
        public static GameObject[] PopupContents = new GameObject[]
        {
            Bundle.LoadAsset<GameObject>("popup-body-1"),
            Bundle.LoadAsset<GameObject>("popup-body-2"),
            Bundle.LoadAsset<GameObject>("popup-body-3"),
            Bundle.LoadAsset<GameObject>("popup-body-4"),
            Bundle.LoadAsset<GameObject>("popup-body-5"),
            Bundle.LoadAsset<GameObject>("popup-body-6"),
            Bundle.LoadAsset<GameObject>("popup-body-7"),
            Bundle.LoadAsset<GameObject>("popup-body-8"),
            Bundle.LoadAsset<GameObject>("popup-body-9")
        };
        public static GameObject AdwareCanvas = Bundle.LoadAsset<GameObject>("Adware UI")
            .AddComponent<PhotonView>().gameObject
            .AddComponent<PhotonMagicInitializer>().gameObject;

        // Cyberpunk
        public static TMPro.TMP_FontAsset CyberFont = Bundle.LoadAsset<TMPro.TMP_FontAsset>("CyberwayRiders-lg97d");
        public static GameObject QuickhackMenu = Bundle.LoadAsset<GameObject>("Quickhack Menu");
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickHacks = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
        {
            { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("Contagion") },
            { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("Short Circuit") },
            { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("Burnout") },
            { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("Hamper") }
        };
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickhackSelectors = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
        {
            { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("ContagionSelector") },
            { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("ShortCircuitSelector") },
            { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("OverheatSelector") },
            { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("HamperSelector") }
        };
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickhackSelectionEffects = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
        {
            { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("ContagionExplosion") },
            { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("ShortCircuitExplosion") },
            { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("OverheatExplosion") },
            { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("HamperExplosion") }
        };
        public static GameObject RamMenu = Bundle.LoadAsset<GameObject>("RAM Menu");
        public static GameObject RamSlot = Bundle.LoadAsset<GameObject>("RAM Slot");

        static Assets()
        {
            GameObject.DontDestroyOnLoad(SnakeSpawner);
            GameObject.DontDestroyOnLoad(BurnoutArt);
            GameObject.DontDestroyOnLoad(ContagionArt);

            var snakeView = SnakePrefab.GetComponent<PhotonView>();
            snakeView.GetComponent<NetworkPhysicsObject>().sendFreq = 2;
            snakeView.Synchronization = ViewSynchronization.UnreliableOnChange;
            snakeView.OwnershipTransfer = OwnershipOption.Takeover;
            snakeView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

            SwordPrefab.GetComponent<SwordBehaviour>().destroyParticles = SwordExplosion;
        }
    }
}
