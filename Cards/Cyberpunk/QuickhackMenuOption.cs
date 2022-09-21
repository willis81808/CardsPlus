using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class QuickhackMenuOption : MonoBehaviour
    {
        public enum QuickhackType
        {
            CONTAGION,
            SHORT_CIRCUIT,
            BURNOUT,
            HAMPER
        }

        public QuickhackType type;
        public GameObject outline;
        public GameObject tooltip;

        public void SetHighlighted(bool highlighted)
        {
            outline.SetActive(highlighted);
            tooltip.SetActive(highlighted);
        }
    }
}
