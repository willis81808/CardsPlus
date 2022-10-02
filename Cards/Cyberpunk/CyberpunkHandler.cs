using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;
using CardsPlusPlugin.Utils;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class CyberpunkHandler : CardEffect
    {
        private void Awake()
        {
            if (QuickhackMenu.Instance == null)
            {
                Instantiate(Assets.QuickhackMenu, Unbound.Instance.canvas.transform);
            }
            if (RamMenu.Instance == null)
            {
                Instantiate(Assets.RamMenu, Unbound.Instance.canvas.transform);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) QuickhackMenu.Toggle();
            if (Input.GetKeyDown(KeyCode.Escape)) QuickhackMenu.Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(QuickhackMenu.Instance.gameObject);
            Destroy(RamMenu.Instance.gameObject);
        }
    }
}
