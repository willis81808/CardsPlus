using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnboundLib;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class CyberpunkHandler : MonoBehaviour
    {
        private void Awake()
        {
            if (QuickhackMenu.Instance == null)
            {
                Instantiate(Assets.QuickhackMenu, Unbound.Instance.canvas.transform);
            }
        }

        private void Update()
        {
            if (QuickhackMenu.Instance == null)
            {
                print("Cannot find Quickhack Menu...");
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Q)) return;

            QuickhackMenu.Toggle();
        }

        private void OnDestroy()
        {
            Destroy(QuickhackMenu.Instance.gameObject);
        }
    }
}
