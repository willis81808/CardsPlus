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

    public class PhotonMagicInitializer<T> : MonoBehaviour, IPunInstantiateMagicCallback where T : PhotonInitializedMonoBehaviour
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            GetComponent<T>().OnInstantiate(info.photonView.InstantiationData);
        }
    }
}
