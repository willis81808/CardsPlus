using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using TMPro;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class QuickhackMenuHelpFormatter : MonoBehaviour
    {
        public string baseText;
        private string textTemplate = "<font=\"LiberationSans SDF\">Press <color=\"red\">{0}</color> to open the </font> <size=+2><color=\"purple\">Quickhack</color></size> <font=\"LiberationSans SDF\"> menu. \n\nScroll <color=\"red\">up & down</color> to cycle through \nyour available</font> <size=+2><color=\"purple\">Quickhacks</color></size><font=\"LiberationSans SDF\">, then <color=\"red\">click</color> to activate the highlighted one.\n\nAfter selecting a</font> <size=+2><color=\"purple\">Quickhack</color></size>  <font=\"LiberationSans SDF\">your targets will glow. <color=\"red\">\nClick</color> any glowing player to install your</font> <size=+2><color=\"purple\">Quickhack</color></size>  <font=\"LiberationSans SDF\">in their system\n\n<color=\"orange\"><i>NOTE: The menu will not open if you do not have enough\n</font><size=+1><color=\"yellow\">RAM</color></size><font=\"LiberationSans SDF\">  to use anything.</i></color></font>";
        private TextMeshProUGUI helperText;

        void Awake()
        {
            helperText = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            helperText.text = string.Format(textTemplate, CardsPlus.quickhackKey.Value.ToString());
        }
    }
}
