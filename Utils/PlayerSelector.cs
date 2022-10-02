using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnityEngine;

namespace CardsPlusPlugin.Utils
{
    /// <summary>
    /// A utility component for detecting and reacting to the user clicking on a player.
    /// 
    /// Note: This component must be attached to a <c>GameObject</c> along with a collider for clicks to be detected.
    ///       Recommend a <c>SphereCollider</c> of approximately radius 3 and center (0, 1, 0); values are based on an object with default scale and no parent objects.
    /// </summary>
    public class PlayerSelector : MonoBehaviour
    {
        private static Dictionary<GameObject, List<PlayerSelector>> selectors = new Dictionary<GameObject, List<PlayerSelector>>();

        private event Action<Player> OnPlayerClicked;
        private bool active = true;

        private void OnDisable()
        {
            Destroy(gameObject);
        }

        private void OnMouseDown()
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

        /// <summary>
        /// <para>Clears all player selectors initialized using a prefab with a name containing <c>basePrefabName</c></para>
        /// <para>Note: This is potentially dangerous, it is recommended to use <see cref="PlayerSelector.Clear(GameObject)"/> instead</para>
        /// </summary>
        /// <param name="basePrefabName">Case sensitive</param>
        public static void Clear(string basePrefabName)
        {
            foreach (var key in selectors.Keys.Where(go => go.name.Contains(basePrefabName)))
            {
                Clear(key);
            }
        }

        /// <summary>
        /// Clears all player selectors initialized using the given prefab
        /// </summary>
        /// <param name="basePrefab"></param>
        public static void Clear(GameObject basePrefab)
        {
            if (selectors.TryGetValue(basePrefab, out List<PlayerSelector> objs))
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i] == null) continue;
                    objs[i].Remove();
                }
                objs.Clear();
            }
        }

        public static void InstantiateOnAll(GameObject selectorPrefab, Action<Player> selectedCallback)
        {
            InstantiateOn(selectorPrefab, selectedCallback, PlayerManager.instance.players);
        }

        public static void InstantiateOnEnemies(GameObject selectorPrefab, Action<Player> selectedCallback)
        {
            int teamId = PlayerManager.instance.players
                .Where(p => p.data.view.IsMine)
                .Select(p => p.teamID)
                .First();

            var targets = PlayerManager.instance.players.Where(p => p.teamID != teamId);

            InstantiateOn(selectorPrefab, selectedCallback, PlayerManager.instance.GetPlayersInTeam(teamId));
        }

        public static void InstantiateOnFriendlies(GameObject selectorPrefab, Action<Player> selectedCallback)
        {
            int teamId = PlayerManager.instance.players
                .Where(p => p.data.view.IsMine)
                .Select(p => p.teamID)
                .First();

            var targets = PlayerManager.instance.players.Where(p => p.teamID == teamId);

            InstantiateOn(selectorPrefab, selectedCallback, targets);
        }

        /// <summary>
        /// Initialize a <see cref="PlayerSelector"/> on each provided <see cref="Player"/>
        /// </summary>
        /// <param name="selectorPrefab">Base prefab with a <see cref="PlayerSelector"/> attached</param>
        /// <param name="selectedCallback">Action to invoke when the user has clicked one of the instantiated <see cref="PlayerSelector"/>s</param>
        /// <param name="forceInitializeAllPlayers">(Optional) if true a <see cref="PlayerSelector"/> will be spawned for every player</param>
        public static void InstantiateOn(GameObject selectorPrefab, Action<Player> selectedCallback, IEnumerable<Player> targets)
        {
            if (selectorPrefab == null)
            {
                CardsPlus.LOGGER.LogError($"[PlayerSelector] Selector prefab cannot  be null!");
                return;
            }

            Clear(selectorPrefab);

            if (selectorPrefab.GetComponentInChildren<PlayerSelector>() == null)
            {
                CardsPlus.LOGGER.LogError($"[PlayerSelector] The selector prefab provided ({selectorPrefab.name}) does not contain an instance of PlayerSelector!");
                return;
            }

            foreach (var player in targets)
            {
                var selector = Instantiate(selectorPrefab, player.transform).GetComponentInChildren<PlayerSelector>();
                selector.OnPlayerClicked += (target) =>
                {
                    selectedCallback?.Invoke(target);
                    Clear(selectorPrefab);
                };

                if (selectors.TryGetValue(selectorPrefab, out List<PlayerSelector> list))
                {
                    list.Add(selector);
                }
                else
                {
                    selectors.Add(selectorPrefab, new List<PlayerSelector> { selector });
                }
            }
        }
    }

}
