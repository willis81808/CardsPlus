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

namespace CardsPlusPlugin
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, "1.0.4")]
    [BepInProcess("Rounds.exe")]
    public class CardsPlus : BaseUnityPlugin
    {
        private const string ModId = "com.willis.rounds.cardsplus";
        private const string ModName = "Cards Plus";
        internal static AssetBundle ArtAssets;
        
        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            ArtAssets = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("cardsplusart", typeof(CardsPlus).Assembly);
            if (ArtAssets == null)
            {
                Debug.Log("Failed to load Cards Plus art asset bundle");
            }
        }
        void Start()
        {
            CustomCard.BuildCard<HareCard>();
            CustomCard.BuildCard<TurtleCard>();
            CustomCard.BuildCard<TerminatorCard>();
            CustomCard.BuildCard<SlowPokeCard>();
            CustomCard.BuildCard<PhantomCard>();
            CustomCard.BuildCard<LowGravityCard>();
            CustomCard.BuildCard<QuickReflexesCard>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                var card = CardChoice.instance.cards.FirstOrDefault(c => c.cardName.ToLower() == "quick reflexes");
                var spawned = PhotonNetwork.Instantiate(card.gameObject.name, Vector3.zero, Quaternion.identity);
                this.ExecuteAfterFrames(10, () =>
                {
                    //CardChoice.instance.Pick(spawned);
                    spawned.GetComponent<ApplyCardStats>().Pick(GetSelf().teamID, false, PickerType.Team);
                    this.ExecuteAfterFrames(3, () => PhotonNetwork.Destroy(spawned));
                });
            }
            
            //if (Input.GetKeyDown(KeyCode.Keypad1))
            //{
            //    var card = CardChoice.instance.cards.FirstOrDefault(c => c.cardName.ToLower() == "poison");
            //    var spawned = PhotonNetwork.Instantiate(card.gameObject.name, Vector3.zero, Quaternion.identity);
            //    this.ExecuteAfterFrames(10, () =>
            //    {
            //        //CardChoice.instance.Pick(spawned);
            //        spawned.GetComponent<ApplyCardStats>().Pick(GetSelf().teamID, false, PickerType.Team);
            //        this.ExecuteAfterFrames(3, () => PhotonNetwork.Destroy(spawned));
            //    });
            //}
        }

        private Player GetSelf()
        {
            var name = PhotonNetwork.LocalPlayer.NickName;
            return (from s in PlayerManager.instance.players
                where s.GetComponent<PhotonView>().Owner.NickName == name
                select s).FirstOrDefault();
        }
    }
}
