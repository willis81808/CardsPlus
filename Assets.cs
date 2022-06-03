using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

namespace CardsPlusPlugin
{
    internal class Assets
    {
        private static readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("cardsplusart", typeof(CardsPlus).Assembly);

        public static GameObject TurtleArt = Bundle.LoadAsset<GameObject>("C_TheTurtle");

        public static GameObject HareArt = Bundle.LoadAsset<GameObject>("C_TheHare");

        public static GameObject TerminatorArt = Bundle.LoadAsset<GameObject>("C_Terminator");

        public static GameObject SlowPokeArt = Bundle.LoadAsset<GameObject>("C_SlowPoke");

        public static GameObject PhantomArt = Bundle.LoadAsset<GameObject>("C_Phantom");

        public static GameObject QuickReflexesArt = Bundle.LoadAsset<GameObject>("C_QuickReflexes");

        public static GameObject LowGravityArt = Bundle.LoadAsset<GameObject>("C_LowGravity");

        public static GameObject ExcaliburArt = Bundle.LoadAsset<GameObject>("C_Excalibur");

        public static GameObject SnakePrefab = Bundle.LoadAsset<GameObject>("Snake").AddComponent<Cards.SnakeFollow>().gameObject.AddComponent<PhotonView>().gameObject.AddComponent<NetworkPhysicsObject>().gameObject;
        public static GameObject SnakeSpawner = new GameObject("Snake Spawner").AddComponent<Cards.SnakeSpawner>().gameObject;

        public static GameObject SwordPrefab = Bundle.LoadAsset<GameObject>("Sword").AddComponent<Cards.SwordBehaviour>().gameObject;
        public static GameObject SwordExplosion = Bundle.LoadAsset<GameObject>("ProtonExplosionYellow");

        static Assets()
        {
            GameObject.DontDestroyOnLoad(SnakeSpawner);

            //SnakePrefab.AddComponent<PhotonTransformView>().m_SynchronizePosition = true;
            //SnakePrefab.transform.GetChild(0).gameObject.AddComponent<PhotonTransformView>().m_SynchronizeRotation = true;
            var snakeView = SnakePrefab.GetComponent<PhotonView>();
            snakeView.GetComponent<NetworkPhysicsObject>().sendFreq = 2;
            snakeView.Synchronization = ViewSynchronization.UnreliableOnChange;
            snakeView.OwnershipTransfer = OwnershipOption.Takeover;
            snakeView.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

            SwordPrefab.GetComponent<Cards.SwordBehaviour>().destroyParticles = SwordExplosion;
        }
    }
}
