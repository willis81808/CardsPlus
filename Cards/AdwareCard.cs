using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib.Cards;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using UnboundLib;
using UnboundLib.Networking;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using UnityEngine.EventSystems;

namespace CardsPlusPlugin.Cards
{
    public class AdwareCard : CustomCard
    {
        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (!player.data.view.IsMine) return;

            player.gameObject.AddComponent<AdwareHandler>();
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (!player.data.view.IsMine) return;

            var adwareHandler = player.gameObject.GetComponent<AdwareHandler>();
            if (adwareHandler)
            {
                Destroy(adwareHandler);
            }
        }

        protected override string GetTitle() => "Adware";
        protected override string GetDescription() => "Press <color=\"purple\">F</color> to enter <color=\"red\">Hacker Mode</color>! When you select a target they'll be bombarded with annoying popup advertisements";
        public override string GetModName() => "Cards+";
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.EvilPurple;
        protected override GameObject GetCardArt() => Assets.AdwareArt;
        protected override CardInfoStat[] GetStats() => null;
    }

    public class AdwareHandler : MonoBehaviour
    {
        private const float COOLDOWN = 15f;
        private const int AD_COUNT = 5;

        private Player player;
        private AdwarePlayerCanvas visuals;
        private bool ready = false;
        private bool cooling = false;

        private void Awake()
        {
            player = GetComponent<Player>();

            var data = new object[] { player.playerID };
            var visualsObj = PhotonNetwork.Instantiate(Assets.AdwareCanvas.name, transform.position, Quaternion.identity, data: data);
            visuals = visualsObj.GetComponent<AdwarePlayerCanvas>();
        }

        private void Start()
        {
            player.data.healthHandler.reviveAction += Reset;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleReady();
            }
        }
        
        private void OnDestroy()
        {
            player.data.healthHandler.reviveAction -= Reset;
            if (ready) ToggleReady();
            PhotonNetwork.Destroy(visuals.GetComponent<PhotonView>());
        }

        private void Reset()
        {
            PlayerSelector.Clear();
            visuals.CooldownValue = 1;
            ready = false;
            cooling = false;
        }

        private void ToggleReady()
        {
            if (cooling) return;

            if (ready)
            {
                PlayerSelector.Clear();
            }
            else
            {
                PlayerSelector.Instantiate(OnPlayerSelected);
            }

            ready = !ready;

            this.ExecuteAfterFrames(1, () => player.data.input.silencedInput = ready);
        }

        private void OnPlayerSelected(Player target)
        {
            ToggleReady();

            // Send hacked network messages
            this.ExecuteAfterSeconds(0.5f, () =>
            {
                NetworkingManager.RPC(typeof(AdwareHandler), nameof(RPC_CreatePopups), AD_COUNT, target.playerID);
                NetworkingManager.RPC(typeof(AdwareHandler), nameof(RPC_ShowHackedFeedback), target.playerID);
            });

            cooling = true;
            StartCoroutine(visuals.CooldownCoroutine(COOLDOWN, () => cooling = false));
        }
        
        [UnboundRPC]
        private static void RPC_ShowHackedFeedback(int playerId)
        {
            var target = PlayerManager.instance.players.Where(p => p.playerID == playerId).FirstOrDefault();
            Instantiate(Assets.HackedTargetEffect, target.transform);
        }

        [UnboundRPC]
        private static void RPC_CreatePopups(int count, int playerId)
        {
            int myPlayerId = PlayerManager.instance.players
                .Where(p => p.data.view.IsMine)
                .Select(p => p.playerID)
                .FirstOrDefault();

            if (myPlayerId != playerId) return;

            for (int i = 0; i < count; i++)
            {
                AdwarePopup.Build();
            }
        }
    }

    public class AdwarePlayerCanvas : PhotonInitializedMonoBehaviour
    {
        public Slider cooldownIndicator;

        public float CooldownValue
        {
            get => cooldownIndicator.value;
            set
            {
                GetComponent<PhotonView>().RPC(nameof(RPC_SetValue), RpcTarget.All, value);
            }
        }

        public void Start()
        {
            cooldownIndicator.value = 1;
            cooldownIndicator.gameObject.SetActive(false);
        }

        public IEnumerator CooldownCoroutine(float duration, Action callback)
        {
            cooldownIndicator.gameObject.SetActive(true);

            CooldownValue = 0;

            float time = Time.time;
            while (Time.time - time < duration)
            {
                CooldownValue = (Time.time - time) / duration;
                yield return new WaitForSecondsRealtime(0.2f);
            }

            CooldownValue = 1;

            cooldownIndicator.gameObject.SetActive(false);

            callback?.Invoke();
        }

        public override void OnInstantiate(object[] data)
        {
            int playerId = (int)data[0];

            var owner = PlayerManager.instance.players.Where(p => p.playerID == playerId).FirstOrDefault();

            transform.position = owner.transform.position;
            transform.parent = owner.transform;
        }

        [PunRPC]
        public void RPC_SetValue(float value)
        {

            cooldownIndicator.value = value;
            cooldownIndicator.gameObject.SetActive(value < 1);
        }
    }

    public class PlayerSelector : MonoBehaviour
    {
        private static List<PlayerSelector> selectors = new List<PlayerSelector>();
        
        public event Action<Player> OnPlayerClicked;

        private bool active = true;

        public void OnMouseDown()
        {
            if (!active) return;

            var player = GetComponentInParent<Player>();
            OnPlayerClicked?.Invoke(player);
        }

        private void Remove()
        {
            active = false;
            GetComponent<ParticleSystem>().Stop();
            Destroy(gameObject, 5f);
        }

        public static void Clear()
        {
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].Remove();
            }
            selectors.Clear();
        }

        public static void Instantiate(Action<Player> selectedCallback)
        {
            int teamId = PlayerManager.instance.players
                .Where(p => p.data.view.IsMine)
                .Select(p => p.teamID)
                .FirstOrDefault();

            foreach (var player in PlayerManager.instance.players)
            {
                if (player.teamID == teamId) continue;

                var selector = Instantiate(Assets.PlayerSelector, player.transform).GetComponent<PlayerSelector>();
                selector.OnPlayerClicked += selectedCallback;
                selectors.Add(selector);
            }
        }
    }

    public class AdwarePopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static Player player => PlayerManager.instance.players.Where(p => p.data.view.IsMine).FirstOrDefault();
        private static Canvas canvas => Unbound.Instance.canvas;

        private static int titlesCounter = 0, contentsCounter = 0;

        private static string[] popupTitles = new string[]
        {
            "WINNER WINNER WINNER!",
            "1,000,000th Visitor!",
            "Congradulations!",
            "You won!!1",
            "Security Warning",
            "You've been infected!",
            "A L E R T",
            "Hackers Detected",
            "Secure your network",
            "Your Windows subscription has expired!",
            "HOT SINGLES IN YOUR AREA"
        };

        public TextMeshProUGUI titleText;
        public RectTransform contentContainer;

        private void Start()
        {
            RandomizeTitle();
            RandomizeContents();
        }

        private void RandomizeTitle()
        {
            titleText.text = popupTitles[titlesCounter++ % popupTitles.Length];
        }
        
        private void RandomizeContents()
        {
            var prefab = Assets.PopupContents[contentsCounter++ % Assets.PopupContents.Length];
            Instantiate(prefab, contentContainer);
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        public void Interact()
        {
            AdwarePopup.Build();
        }

        public static AdwarePopup Build()
        {
            var popup = Instantiate(Assets.PopupPrefab, canvas.transform).GetComponent<AdwarePopup>();
            var popupTransform = (RectTransform)popup.transform;

            int maxDeltaX = (Screen.width / 2) - 500;
            int maxDeltaY = (Screen.height / 2) - 400;

            popupTransform.anchoredPosition += new Vector2(Random.Range(-maxDeltaX, maxDeltaX), Random.Range(-maxDeltaY, maxDeltaY));

            return popup;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            player.data.input.silencedInput = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            player.data.input.silencedInput = false;
        }
    }
}
