using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    public class PlayerSelector : MonoBehaviour
    {
        private static List<PlayerSelector> selectors = new List<PlayerSelector>();

        public event Action<Player> OnPlayerClicked;

        private bool active = true;

        public void OnDisable()
        {
            Destroy(gameObject);
        }

        public void OnMouseDown()
        {
            if (!active) return;

            var player = GetComponentInParent<Player>();
            OnPlayerClicked?.Invoke(player);
        }

        private void Remove()
        {
            active = false;

            if (!gameObject.activeInHierarchy)
            {
                Destroy(gameObject);
                return;
            }

            GetComponent<ParticleSystem>()?.Stop();
            Destroy(gameObject, 5f);
        }

        public static void Clear()
        {
            for (int i = 0; i < selectors.Count; i++)
            {
                if (selectors[i] == null) continue;
                selectors[i].Remove();
            }
            selectors.Clear();
        }

        public static void Instantiate(GameObject selectorPrefab, Action<Player> selectedCallback)
        {
            Clear();

            int teamId = PlayerManager.instance.players
                .Where(p => p.data.view.IsMine)
                .Select(p => p.teamID)
                .FirstOrDefault();

            foreach (var player in PlayerManager.instance.players)
            {
                if (!CardsPlus.allowSelfTargeting.Value && player.teamID == teamId) continue;

                var selector = Instantiate(selectorPrefab, player.transform).GetComponent<PlayerSelector>();
                selector.OnPlayerClicked += (target) =>
                {
                    selectedCallback?.Invoke(target);
                    Clear();
                };
                selectors.Add(selector);
            }
        }
    }

}
