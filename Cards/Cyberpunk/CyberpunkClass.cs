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
            CardsPlus.LOGGER.LogInfo("[CyberpunkClass] Start registering");

            CardInfo
                shortCircuit = null,
                hamper = null,
                burnout = null,
                contagion = null,
                ramUpgrade = null,
                overclock = null,
                cyberPsychosis = null;

            while (!(shortCircuit && hamper && burnout && contagion && ramUpgrade && overclock && cyberPsychosis))
            {
                try
                {
                    shortCircuit = CardRegistry.GetCard<ShortCircuitCard>();
                    hamper = CardRegistry.GetCard<HamperCard>();
                    burnout = CardRegistry.GetCard<BurnoutCard>();
                    contagion = CardRegistry.GetCard<ContagionCard>();
                    ramUpgrade = CardRegistry.GetCard<RamUpgradeCard>();
                    overclock = CardRegistry.GetCard<OverclockCard>();
                    cyberPsychosis = CardRegistry.GetCard<CyberPsychosisCard>();
                }
                catch (Exception e)
                {
                    CardsPlus.LOGGER.LogWarning("Part 1");
                    CardsPlus.LOGGER.LogError(e.Message);
                    CardsPlus.LOGGER.LogError(e.StackTrace);
                }
                yield return null;
            }

            try
            {
                // layer one
                ClassesRegistry.Register(shortCircuit, CardType.NonClassCard | CardType.Entry);
                ClassesRegistry.Register(hamper, CardType.NonClassCard | CardType.Entry);
                ClassesRegistry.Register(contagion, CardType.NonClassCard | CardType.Entry);
                ClassesRegistry.Register(burnout, CardType.NonClassCard | CardType.Entry);

                // require one
                var layerOne = new[] { shortCircuit, hamper, burnout, contagion };
                ClassesRegistry.Register(ramUpgrade, CardType.NonClassCard | CardType.Card, ArrayOfElementArrays(layerOne), 3);
                ClassesRegistry.Register(overclock, CardType.NonClassCard | CardType.Card, ArrayOfElementArrays(layerOne), 2);

                // require all
                ClassesRegistry.Register(cyberPsychosis, CardType.NonClassCard | CardType.Card, layerOne);

                CardsPlus.LOGGER.LogInfo("[CyberpunkClass] Finished registering");
            }
            catch (Exception e)
            {
                CardsPlus.LOGGER.LogWarning("Part 2");
                CardsPlus.LOGGER.LogError(e.Message);
                CardsPlus.LOGGER.LogError(e.StackTrace);
            }
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
