using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class QuickhackMenu : MonoBehaviour
    {
        public static QuickhackMenu Instance { get; private set; }
        public static bool Active { get; private set; } = false;

        private List<QuickhackMenuOption> availableHacks = new List<QuickhackMenuOption>();

        private MouseDeltaTracker mouseData;
        private Player player;

        private float horizontalMouseDelta = 0f;
        private float mouseDeltaThreshold = Screen.width / 15;
        private int highlightedIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }
            Instance = this;

            mouseData = gameObject.AddComponent<MouseDeltaTracker>();
            player = PlayerManager.instance.players.Where(p => p.data.view.IsMine).First();

            Hide();
        }

        private void LateUpdate()
        {
            if (!Active) return;

            if (Input.GetMouseButtonDown(0))
            {
                print($"Activated quickhack: {availableHacks[highlightedIndex].type}");
                Hide();
                return;
            }

            horizontalMouseDelta += mouseData.GetDelta().x;

            if (Math.Abs(horizontalMouseDelta) >= mouseDeltaThreshold)
            {
                MoveSelection(horizontalMouseDelta < 0 ? highlightedIndex + 1 : highlightedIndex - 1);
                horizontalMouseDelta = 0;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void MoveSelection(int newIndex)
        {
            if (newIndex < 0 || newIndex >= availableHacks.Count || highlightedIndex == newIndex) return;

            SetSelection(newIndex);
        }

        private void SetSelection(int newIndex)
        {
            for (int i = 0; i < availableHacks.Count; i++)
            {
                availableHacks[i].SetHighlighted(i == newIndex);
            }

            highlightedIndex = newIndex;
        }

        public static void AddQuickhack(QuickhackMenuOption.QuickhackType type)
        {
            var prefab = Assets.QuickHacks[type];
            var menuOption = Instantiate(prefab, Instance.transform).GetComponent<QuickhackMenuOption>();
            Instance.availableHacks.Add(menuOption);
        }

        public static void RemoveQuickhack(QuickhackMenuOption.QuickhackType type)
        {
            var menuOption = Instance.availableHacks.Where(qh => qh.type == type).FirstOrDefault();
            if (!menuOption) return;

            Instance.availableHacks.Remove(menuOption);
            Destroy(menuOption.gameObject);
        }

        public static void Toggle()
        {
            if (Active) Hide();
            else Show();
        }

        public static void Show()
        {
            Active = true;
            Instance.player.data.input.silencedInput = true;

            Instance.SetSelection(0);
            Instance.gameObject.SetActive(Active);
        }

        public static void Hide()
        {
            Active = false;
            if (Instance.player.data.silenceTime == 0) Instance.player.data.input.silencedInput = false;

            Instance.horizontalMouseDelta = 0;
            Instance.gameObject.SetActive(Active);
        }
    }

    public class MouseDeltaTracker : MonoBehaviour
    {
        private Vector3 lastPosition;
        private Vector3 delta;

        private void Update()
        {
            delta = lastPosition - Input.mousePosition;
            lastPosition = Input.mousePosition;
        }

        public Vector3 GetDelta()
        {
            return delta;
        }
    }
}
