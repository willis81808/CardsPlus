using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsPlusPlugin.Utils;
using ClassesManagerReborn;
using ModsPlus;
using UnboundLib.Cards;
using UnityEngine;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class CyberpunkClass : ClassHandler
    {
        public override IEnumerator Init()
        {
            CardInfo
                shortCircuit = null,
                hamper = null,
                burnout = null,
                contagion = null,
                rebootOptics = null,
                ramUpgrade = null,
                overclock = null,
                cyberPsychosis = null;

            while (!(shortCircuit && hamper && burnout && contagion && rebootOptics && ramUpgrade && overclock && cyberPsychosis))
            {
                shortCircuit = CardRegistry.GetCard<ShortCircuitCard>();
                hamper = CardRegistry.GetCard<HamperCard>();
                burnout = CardRegistry.GetCard<BurnoutCard>();
                contagion = CardRegistry.GetCard<ContagionCard>();
                rebootOptics = CardRegistry.GetCard<RebootOpticsCard>();
                ramUpgrade = CardRegistry.GetCard<RamUpgradeCard>();
                overclock = CardRegistry.GetCard<OverclockCard>();
                cyberPsychosis = CardRegistry.GetCard<CyberPsychosisCard>();
                yield return null;
            }

            // Register quickhacks as entrypoints with each other quickhack whitelisted
            var quickhacks = new[] { shortCircuit, hamper, burnout, contagion, rebootOptics };
            RegisterAndWhitelistAll(quickhacks);

            var oneOfAny = ArrayOfElementArrays(quickhacks);
            ClassesRegistry.Register(ramUpgrade, CardType.Card, oneOfAny, 3);
            ClassesRegistry.Register(overclock, CardType.Card, oneOfAny, 2);

            ClassesRegistry.Register(cyberPsychosis, CardType.Card, quickhacks);
        }

        private void RegisterAndWhitelistAll(params CardInfo[] cards)
        {
            foreach (var c in cards)
            {
                List<CardInfo> others = new List<CardInfo>(cards);
                others.Remove(c);
                RegisterWithWhitelist(c, others.ToArray());
            }
        }

        private void RegisterWithWhitelist(CardInfo source, params CardInfo[] cardsToWhitelist)
        {
            var classObj = ClassesRegistry.Register(source, CardType.Entry);
            foreach (var ci in cardsToWhitelist) classObj.Whitelist(ci);
        }

        private CardInfo[][] ArrayOfElementArrays(params CardInfo[] cards)
        {
            CardInfo[][] result = new CardInfo[cards.Length][];
            for (int i = 0; i < cards.Length; i ++)
            {
                result[i] = new[] { cards[i] };
            }
            return result;
        }
    }
}
