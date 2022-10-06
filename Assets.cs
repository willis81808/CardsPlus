using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static event Action OnFinishedLoadingAssets;

        private static AssetBundle Bundle;

        //
        // CARD ART
        //
        public static GameObject TurtleArt;
        public static GameObject HareArt;
        public static GameObject TerminatorArt;
        public static GameObject SlowPokeArt;
        public static GameObject PhantomArt;
        public static GameObject QuickReflexesArt;
        public static GameObject LowGravityArt;
        public static GameObject SnakeAttackArt;
        public static GameObject ExcaliburArt;
        public static GameObject HotPotatoArt;
        public static GameObject SmokeGrenadeArt;
        public static GameObject AdwareArt;
        public static GameObject ShortCircuitArt;
        public static GameObject HamperArt;
        public static GameObject ContagionArt;
        public static GameObject BurnoutArt;

        // Snake attack
        public static GameObject SnakePrefab;

        // Excalibur
        public static GameObject SwordPrefab;
        public static GameObject SwordExplosion;

        // Hot potato
        public static GameObject FlameArea;

        // Smoke Grenade
        public static GameObject SmokeObject;
        public static GameObject SmokeEffect;

        // Adware
        public static GameObject AdwarePlayerSelector;
        public static GameObject PopupPrefab;
        public static GameObject HackedTargetEffect;
        public static GameObject[] PopupContents;
        public static GameObject AdwareCanvas;

        // Cyberpunk
        public static TMPro.TMP_FontAsset CyberFont;
        public static Material GlitchShaderMaterialHolder;
        public static GameObject GlitchedParticleEffect;
        public static GameObject ElectricFrame;
        public static GameObject QuickhackMenu;
        public static GameObject RamMenu;
        public static GameObject RamSlot;
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickHacks;
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickhackSelectors;
        public static Dictionary<QuickhackMenuOption.QuickhackType, GameObject> QuickhackSelectionEffects;

        static Assets()
        {
            UnboundLib.Unbound.Instance.StartCoroutine(LoadAssets());
        }

        public static IEnumerator LoadAssetBundleFromResources(bool doAsync, string bundleName, Assembly resourceAssembly)
        {
            if (resourceAssembly == null)
            {
                throw new ArgumentNullException("Parameter resourceAssembly can not be null.");
            }

            string resourceName = null;
            try
            {
                resourceName = resourceAssembly.GetManifestResourceNames().Single(str => str.EndsWith(bundleName));
            }
            catch (Exception) { }

            if (resourceName == null)
            {
                CardsPlus.LOGGER.LogError($"AssetBundle {bundleName} not found in assembly manifest");
                yield break;
            }

            AssetBundleCreateRequest ret;
            using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
            {
                if (doAsync)
                {
                    ret = AssetBundle.LoadFromStreamAsync(stream);
                    yield return new WaitUntil(() => ret.isDone);
                    Bundle = ret.assetBundle;
                }
                else
                {
                    Bundle = AssetBundle.LoadFromStream(stream);
                }
            }
        }

        private static IEnumerator LoadAssets()
        {
            CardsPlus.LOGGER.LogInfo("STARTED LOADING BUNDLE");

            yield return LoadAssetBundleFromResources(false, "cardsplusart", typeof(CardsPlus).Assembly);

            CardsPlus.LOGGER.LogInfo("FINISHED LOADING BUNDLE");

            TurtleArt = Bundle.LoadAsset<GameObject>("C_TheTurtle");
            HareArt = Bundle.LoadAsset<GameObject>("C_TheHare");
            TerminatorArt = Bundle.LoadAsset<GameObject>("C_Terminator");
            SlowPokeArt = Bundle.LoadAsset<GameObject>("C_SlowPoke");
            PhantomArt = Bundle.LoadAsset<GameObject>("C_Phantom");
            QuickReflexesArt = Bundle.LoadAsset<GameObject>("C_QuickReflexes");
            LowGravityArt = Bundle.LoadAsset<GameObject>("C_LowGravity");
            ExcaliburArt = Bundle.LoadAsset<GameObject>("C_Excalibur");
            SmokeGrenadeArt = Bundle.LoadAsset<GameObject>("C_SmokeGrenade");
            AdwareArt = Bundle.LoadAsset<GameObject>("C_Adware");
            SnakeAttackArt = Bundle.LoadAsset<GameObject>("C_SnakeAttack");
            HotPotatoArt = Bundle.LoadAsset<GameObject>("C_HotPotato");
            ElectricFrame = Bundle.LoadAsset<GameObject>("Short Circuit Frame");
            ShortCircuitArt = Bundle.LoadAsset<GameObject>("C_ShortCircuit").AddComponent<CyberCardEffect>().gameObject;
            HamperArt = Bundle.LoadAsset<GameObject>("C_Hamper").AddComponent<CyberCardEffect>().gameObject;
            ContagionArt = Bundle.LoadAsset<GameObject>("C_Contagion").AddComponent<CyberCardEffect>().gameObject;
            BurnoutArt = Bundle.LoadAsset<GameObject>("C_Burnout").AddComponent<CyberCardEffect>().gameObject;

            SnakePrefab = Bundle.LoadAsset<GameObject>("Snake").AddComponent<SnakeFollow>().gameObject;
            var snakeView = SnakePrefab.gameObject.AddComponent<PhotonView>();
            snakeView.Synchronization = ViewSynchronization.UnreliableOnChange;
            snakeView.OwnershipTransfer = OwnershipOption.Takeover;
            snakeView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
            var snakePhysics = SnakePrefab.gameObject.AddComponent<NetworkPhysicsObject>();
            snakePhysics.sendFreq = 2;
            snakePhysics.collisionThreshold = float.PositiveInfinity;
            snakePhysics.playerColThreshold = float.PositiveInfinity;
            snakePhysics.shakeAmount = 0;
            snakePhysics.bulletPushMultiplier = 10;
            snakePhysics.dmgAmount = 0;
            snakePhysics.forceAmount = 0;

            // Excalibur
            SwordPrefab = Bundle.LoadAsset<GameObject>("Sword").AddComponent<SwordBehaviour>().gameObject;
            SwordPrefab.GetComponent<SwordBehaviour>().destroyParticles = SwordExplosion;
            SwordExplosion = Bundle.LoadAsset<GameObject>("ProtonExplosionYellow");

            // Hot potato
            FlameArea = Bundle.LoadAsset<GameObject>("FlameArea").AddComponent<HotPotatoFlame>().gameObject;

            // Smoke Grenade
            SmokeObject = Bundle.LoadAsset<GameObject>("Smoke Object").AddComponent<PhotonView>().gameObject;
            SmokeEffect = Bundle.LoadAsset<GameObject>("Smoke Effect");

            // Adware
            AdwarePlayerSelector = Bundle.LoadAsset<GameObject>("Player Selector");
            PopupPrefab = Bundle.LoadAsset<GameObject>("Popup Window");
            HackedTargetEffect = Bundle.LoadAsset<GameObject>("HackedTargetEffect");
            PopupContents = new GameObject[]
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
            AdwareCanvas = Bundle.LoadAsset<GameObject>("Adware UI")
                .AddComponent<PhotonView>().gameObject
                .AddComponent<PhotonMagicInitializer>().gameObject;

            // Cyberpunk
            GlitchedParticleEffect = Bundle.LoadAsset<GameObject>("GlitchedEffect");
            GlitchShaderMaterialHolder = Bundle.LoadAsset<Material>("GlitchMaterial");
            CyberFont = Bundle.LoadAsset<TMPro.TMP_FontAsset>("CyberwayRiders-lg97d");
            QuickhackMenu = Bundle.LoadAsset<GameObject>("Quickhack Menu");
            QuickHacks = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
            {
                { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("Contagion") },
                { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("Short Circuit") },
                { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("Burnout") },
                { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("Hamper") }
            };
            QuickhackSelectors = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
            {
                { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("ContagionSelector") },
                { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("ShortCircuitSelector") },
                { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("OverheatSelector") },
                { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("HamperSelector") }
            };
            QuickhackSelectionEffects = new Dictionary<QuickhackMenuOption.QuickhackType, GameObject>
            {
                { QuickhackMenuOption.QuickhackType.CONTAGION,      Bundle.LoadAsset<GameObject>("ContagionExplosion") },
                { QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT,  Bundle.LoadAsset<GameObject>("ShortCircuitExplosion") },
                { QuickhackMenuOption.QuickhackType.BURNOUT,        Bundle.LoadAsset<GameObject>("OverheatExplosion") },
                { QuickhackMenuOption.QuickhackType.HAMPER,         Bundle.LoadAsset<GameObject>("HamperExplosion") }
            };
            RamMenu = Bundle.LoadAsset<GameObject>("RAM Menu");
            RamSlot = Bundle.LoadAsset<GameObject>("RAM Slot");

            OnFinishedLoadingAssets?.Invoke();
        }
    }
}
