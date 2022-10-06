using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Extensions;
using UnboundLib.Cards;

namespace CardsPlusPlugin.Utils
{
    public static class CardRegistry
    {
        private static Dictionary<Type, CardInfo> storedCardInfo = new Dictionary<Type, CardInfo>();

        public static void RegisterCard<T>() where T : CustomCard
        {
            CustomCard.BuildCard<T>(StoreCard<T>);
        }

        private static void StoreCard<T>(CardInfo card) where T : CustomCard
        {
            storedCardInfo.Add(typeof(T), card);
        }

        public static CardInfo GetCard<T>() where T : CustomCard
        {
            if (storedCardInfo.TryGetValue(typeof(T), out CardInfo value))
            {
                return value;
            }

            return null;
        }
    }
}
