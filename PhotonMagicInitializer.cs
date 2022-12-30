using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Photon.Pun;

namespace CardsPlusPlugin
{
    public abstract class PhotonInitializedMonoBehaviour : MonoBehaviour
    {
        public abstract void OnInstantiate(object[] data);
    }

    public class PhotonMagicInitializer : MonoBehaviour, IPunInstantiateMagicCallback
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            GetComponent<PhotonInitializedMonoBehaviour>().OnInstantiate(info.photonView.InstantiationData);

            var s = new CardInfoStat[]
            {
                new CardInfoStat
                {
                    positive = true,
                    stat = "Health",
                    amount = "+20%",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }
    }
}
