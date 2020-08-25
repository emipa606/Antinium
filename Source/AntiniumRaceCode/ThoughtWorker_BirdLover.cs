using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace AntiniumRaceCode
{
    public class ThoughtWorker_BirdLoverApparel : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            string text = null;
            int num = 0;
            List<Apparel> wornApparel = p.apparel.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                if (wornApparel[i].Stuff?.defName == "Leather_Bird")
                {
                    if (text == null)
                    {
                        text = wornApparel[i].def.label;
                    }
                    num++;
                }
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



    public class ThoughtWorker_BirdLoverSawBird : ThoughtWorker
    {
        private const float radius = 12f;

        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (pawn == null) { return false; }

            if (!pawn.Spawned || !pawn.RaceProps.Humanlike)
            {
                return false;
            }

            if (pawn?.story?.traits == null)
            {
                return false;
            }

            TraitDef birdLover = DefDatabase<TraitDef>.GetNamed("Ant_BirdLover");

            if (!pawn.story.traits.HasTrait(birdLover))
            {
                return false;
            }

            List<Pawn> mapPawns = new List<Pawn>();
            mapPawns = pawn.Map.mapPawns.AllPawnsSpawned;
            int count = mapPawns.Count();

            List<Pawn> birdPawns = new List<Pawn>();

           
            birdPawns = mapPawns.Where(b => b.RaceProps?.body?.defName == "Bird").ToList();

            if (birdPawns.Count > 0)
            {

                int birds = 0;
                birds = birdPawns.Count(c => c.Position.InHorDistOf(pawn.Position, radius));

                if (birds > 0)
                {

                    return ThoughtState.ActiveAtStage(Math.Min(birds - 1, 4));
                }
            }

            //Log.Message("NO BIRDS on map");

            return false;

        }
    }



}
