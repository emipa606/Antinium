using RimWorld;
using Verse;

namespace AntiniumRaceCode
{
    public class ThoughtWorker_BirdLoverApparel : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            string text = null;
            var num = 0;
            var wornApparel = p.apparel.WornApparel;
            foreach (var apparel in wornApparel)
            {
                if (apparel.Stuff?.defName != "Leather_Bird")
                {
                    continue;
                }

                if (text == null)
                {
                    text = apparel.def.label;
                }

                num++;
            }


            if (num == 0)
            {
                return ThoughtState.Inactive;
            }

            if (num >= 5)
            {
                return ThoughtState.ActiveAtStage(4, text);
            }

            return ThoughtState.ActiveAtStage(num - 1, text);
        }
    }
}