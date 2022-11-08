using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsPlusPlugin.Utils
{
    public class StatBuilder
    {
        private List<CardInfoStat> stats;

        private CardInfoStat currentStat = null;

        private StatBuilder()
        {
            stats = new List<CardInfoStat>();
        }

        public StatBuilder Positive(bool positive)
        {
            VerifyCurrent();
            currentStat.positive = positive;
            return this;
        }

        public StatBuilder Stat(string stat)
        {
            VerifyCurrent();
            currentStat.stat = stat;
            return this;
        }

        public StatBuilder Amount(string amount)
        {
            VerifyCurrent();
            currentStat.amount = amount;
            return this;
        }

        public StatBuilder SimpleAmount(CardInfoStat.SimpleAmount simpleAmount)
        {
            VerifyCurrent();
            currentStat.simepleAmount = simpleAmount;
            return this;
        }

        public StatBuilder Next()
        {
            if (currentStat == null) return this;

            stats.Add(currentStat);
            currentStat = null;
            
            return this;
        }

        public CardInfoStat[] Build()
        {
            Next();
            var result = stats.ToArray();
            stats.Clear();
            return result;
        }


        private void VerifyCurrent()
        {
            if (currentStat == null)
            {
                currentStat = new CardInfoStat() { simepleAmount = CardInfoStat.SimpleAmount.notAssigned };
            }
        }

        public static StatBuilder Builder()
        {
            return new StatBuilder();
        }
    }
}
