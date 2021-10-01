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
    [BepInPlugin(ModId, ModName, "1.1.2")]
    [BepInProcess("Rounds.exe")]
    public class CardsPlus : BaseUnityPlugin
    {
        private const string ModId = "com.willis.rounds.cardsplus";
        private const string ModName = "Cards Plus";
        
        void Awake()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            CustomCard.BuildCard<HareCard>();
            CustomCard.BuildCard<TurtleCard>();
            CustomCard.BuildCard<TerminatorCard>();
            CustomCard.BuildCard<SlowPokeCard>();
            CustomCard.BuildCard<PhantomCard>();
            CustomCard.BuildCard<QuickReflexesCard>();
            CustomCard.BuildCard<LowGravityCard>();
            CustomCard.BuildCard<SnakeAttackCard>();
        }
        
    }
}
