﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnboundLib.Networking;
using UnboundLib;
using CardsPlusPlugin.Utils;

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
                var quickHackType = availableHacks[highlightedIndex].type;
                PlayerSelector.Instantiate(Assets.QuickhackSelectors[quickHackType], (target) => OnPlayerSelected(target, quickHackType));

                Hide();
                return;
            }

            horizontalMouseDelta += mouseData.GetDelta().x;

            if (Math.Abs(horizontalMouseDelta) >= mouseDeltaThreshold)
            {
                MoveSelection(horizontalMouseDelta < 0);
                horizontalMouseDelta = 0;
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void MoveSelection(bool right)
        {
            int newIndex = highlightedIndex + (right ? 1 : -1);
            MoveSelection(newIndex, right);
        }
        private void MoveSelection(int newIndex, bool right)
        {
            if (newIndex < 0 || newIndex >= availableHacks.Count || highlightedIndex == newIndex) return;

            var cost = QuickhackMenuOption.Costs[availableHacks[newIndex].type];
            if (cost > RamMenu.AvailableRam)
            {
                MoveSelection(newIndex + (right ? 1 : -1), right);
                return;
            }

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
            if (Instance == null) return;

            var prefab = Assets.QuickHacks[type];
            var menuOption = Instantiate(prefab, Instance.transform).GetComponent<QuickhackMenuOption>();
            Instance.availableHacks.Add(menuOption);
        }

        public static void RemoveQuickhack(QuickhackMenuOption.QuickhackType type)
        {
            if (Instance == null) return;

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

            int index = Instance.availableHacks.FindIndex(qh => QuickhackMenuOption.Costs[qh.type] <= RamMenu.AvailableRam);
            if (index < 0)
            {
                Hide();
                return;
            }

            Instance.SetSelection(index);
            Instance.gameObject.SetActive(Active);
        }

        public static void Hide()
        {
            Active = false;
            if (Instance.player.data.silenceTime == 0) Instance.player.data.input.silencedInput = false;

            Instance.horizontalMouseDelta = 0;
            Instance.gameObject.SetActive(Active);
        }

        private static void OnPlayerSelected(Player target, QuickhackMenuOption.QuickhackType quickhackType)
        {
            if (target.data.dead) return;

            if (!RamMenu.SpendRam(QuickhackMenuOption.Costs[quickhackType])) return;

            NetworkingManager.RPC(typeof(QuickhackMenu), nameof(RPC_ShowPlayerSelectedEffect), target.playerID, quickhackType);

            switch (quickhackType)
            {
                case QuickhackMenuOption.QuickhackType.CONTAGION:
                    ContagionCard.DoQuickHack(target);
                    break;
                case QuickhackMenuOption.QuickhackType.SHORT_CIRCUIT:
                    ShortCircuitCard.DoQuickHack(target);
                    break;
                case QuickhackMenuOption.QuickhackType.BURNOUT:
                    BurnoutCard.DoQuickHack(target);
                    break;
                case QuickhackMenuOption.QuickhackType.HAMPER:
                    HamperCard.DoQuickHack(target);
                    break;
            }
        }

        [UnboundRPC]
        public static void RPC_ShowPlayerSelectedEffect(int playerId, QuickhackMenuOption.QuickhackType quickhackType)
        {
            var player = PlayerManager.instance.players.Where(p => p.playerID == playerId).First();
            var prefab = Assets.QuickhackSelectionEffects[quickhackType];

            Instantiate(prefab).AddComponent<FollowTarget>().Initialize(player.transform, prefab.transform.position);
        }
    }
}
