using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
