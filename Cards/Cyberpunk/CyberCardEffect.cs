using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace CardsPlusPlugin.Cards.Cyberpunk
{
    public class CyberCardEffect : MonoBehaviour
    {
        public Transform root { get; private set; }

        private bool initialized = false;

        private void Awake()
        {
            var cardInfo = GetComponentInParent<CardInfo>();
            if (!cardInfo) return;

            root = cardInfo.transform;

            initialized = true;
        }

        private void Start()
        {
            if (!initialized) return;

            SetFont();
            CreateFrame();
        }

        private void SetFont()
        {
            var titleText = root.GetChild(0).Find("Canvas/Front/Text_Name").GetComponent<TextMeshProUGUI>();
            titleText.font = Assets.CyberFont;
        }

        public void CreateFrame()
        {
            var canvas = root.GetComponentInChildren<Canvas>();
            Instantiate(Assets.ElectricFrame, canvas.transform);
        }
    }
}
