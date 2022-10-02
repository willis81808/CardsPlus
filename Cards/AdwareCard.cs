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
using CardsPlusPlugin.Utils;
using ModdingUtils.AIMinion.Extensions;

namespace CardsPlusPlugin.Cards
{
    public class AdwareCard : CustomEffectCard<AdwareHandler>
    {
        public override CardDetails Details => new CardDetails
        {
            Title       = "Adware",
            Description = "Press <color=\"purple\">F</color> to enter <color=\"red\">Hacker Mode</color>! When you select a target they'll be bombarded with annoying popup advertisements",
            ModName     = "Cards+",
            Art         = Assets.AdwareArt,
            Rarity      = CardInfo.Rarity.Uncommon,
            Theme       = CardThemeColor.CardThemeColorType.EvilPurple,
            OwnerOnly   = true
        };

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
        }
    }

    public class AdwareHandler : CardEffect
    {
        private const float COOLDOWN = 15f;
        private const int AD_COUNT = 5;

        private AdwarePlayerCanvas visuals;
        private bool ready = false;
        private bool cooling = false;

        private Coroutine coolingRoutine;

        protected override void Start()
        {
            base.Start();

            // create Adware UI
            var data = new object[] { player.playerID };
            var visualsObj = PhotonNetwork.Instantiate(Assets.AdwareCanvas.name, transform.position, Quaternion.identity, data: data);
            visuals = visualsObj.GetComponent<AdwarePlayerCanvas>();

            Reset();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                ToggleReady();
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ready) ToggleReady();
            PhotonNetwork.Destroy(visuals.GetComponent<PhotonView>());
        }

        public override void OnRevive()
        {
            Reset();
        }

        private void Reset()
        {
            ClearAdwareSelectors();
            ready = false;
            cooling = true;
            visuals.CooldownValue = 0;
            StartCooldown();
        }

        public static void ResetCooldown()
        {
            foreach (var handler in FindObjectsOfType<AdwareHandler>())
            {
                handler.Reset();
            }
        }

        private void ToggleReady()
        {
            if (cooling) return;

            if (ready)
            {
                ClearAdwareSelectors();
            }
            else
            {
                if (CardsPlus.allowSelfTargeting.Value)
                {
                    PlayerSelector.InstantiateOnAll(Assets.AdwarePlayerSelector, OnPlayerSelected);
                }
                else
                {
                    PlayerSelector.InstantiateOnEnemies(Assets.AdwarePlayerSelector, OnPlayerSelected);
                }
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
            StartCooldown();
        }

        private void StartCooldown()
        {
            if (coolingRoutine != null) StopCoroutine(coolingRoutine);
            coolingRoutine = StartCoroutine(visuals.CooldownCoroutine(COOLDOWN, () => cooling = false));
        }
        
        public static void ClearAdwareSelectors()
        {
            PlayerSelector.Clear(Assets.AdwarePlayerSelector);
        }

        [UnboundRPC]
        private static void RPC_ShowHackedFeedback(int playerId)
        {
            var target = PlayerManager.instance.players.Where(p => p.playerID == playerId).FirstOrDefault();
            var prefab = Assets.HackedTargetEffect;
            Instantiate(prefab).AddComponent<FollowTarget>().Initialize(target.transform, prefab.transform.position);
        }

        [UnboundRPC]
        private static void RPC_CreatePopups(int count, int playerId)
        {
            var myPlayer = PlayerManager.instance.players
                .Where(p => p.playerID == playerId)
                .FirstOrDefault();

            if (!myPlayer || !myPlayer.data.view.IsMine) return;

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

    public class AdwarePopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static Player player => PlayerManager.instance.players.Where(p => p.data.view.IsMine && !p.data.GetAdditionalData().isAIMinion).FirstOrDefault();
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
            PlayerManager.instance.AddPlayerDiedAction(OnPlayerDeath);
        }

        private void OnDestroy()
        {
            PlayerManager.instance.RemovePlayerDiedAction(OnPlayerDeath);
        }

        private void OnPlayerDeath(Player deadPlayer, int deadPlayersCount)
        {
            if (player.playerID != deadPlayer.playerID || this == null) return;

            Close();
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
