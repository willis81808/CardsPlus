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
        public static Dictionary<QuickhackType, int> Costs = new Dictionary<QuickhackType, int>
        {
            { QuickhackType.CONTAGION, 3 },
            { QuickhackType.SHORT_CIRCUIT, 2 },
            { QuickhackType.BURNOUT, 3 },
            { QuickhackType.HAMPER, 2 }
        };

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

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Update()
        {
            bool purchasable = Costs[type] <= RamMenu.AvailableRam;
            canvasGroup.alpha = purchasable ? 1f : 0.2f;
        }

        public void SetHighlighted(bool highlighted)
        {
            outline.SetActive(highlighted);
            tooltip.SetActive(highlighted);
        }
    }
}
