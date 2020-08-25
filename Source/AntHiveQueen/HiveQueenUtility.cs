using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;


namespace AntiniumHiveQueen
{
    public static class HiveQueenUtility
    {

        // general sensitivity to queen effects
        public static int GetPawnHQScore(Pawn pawn, bool antsOnly = false, bool reqForAnts = true)
        {
            int factor = -1;

            if (antsOnly && !(pawn.kindDef.race.defName == "Ant_AntiniumRace"))
            {
                return -1;
            }

            // default increase for ants
            if (pawn.kindDef.race.defName == "Ant_AntiniumRace")
            {
                factor = 2;
            }

            // psychic sensitivity
            float hyper = pawn.GetStatValue(StatDefOf.PsychicSensitivity, true);


            if (hyper <= .3)
            {
                factor -= 2;
            }

            else if (hyper <= .6)
            {
                factor -= 1;
            }

            if (hyper >= 1.7)
            {
                factor += 2;
            }

            else if (hyper >= 1.3)
            {
                factor += 1;
            }

            // Min 0 pt for ants, if req.
            if (reqForAnts && pawn.kindDef.race.defName == "Ant_AntiniumRace")
            {
                factor = Math.Max(factor, 0);
            }

            return factor;
        }




        public static bool QueenExistsOnMap(Map map)
        {
            List<Pawn> list = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.RaceProps.Humanlike).ToList();
            
            for (int i = 0; i < list.Count(); i++)
            {
                Pawn pawn = list[i];
                CompHQPresence comp = pawn.TryGetComp<CompHQPresence>();
                if (comp != null)
                {
                    if (comp.Active)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool QueenExists()
        {
            List<Pawn> list = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.ToList();

            for (int i = 0; i < list.Count(); i++)
            {
                Pawn pawn = list[i];
                CompHQPresence comp = pawn.TryGetComp<CompHQPresence>();
                if (comp != null)
                {
                    if (comp.Active)
                    {
                        return true;
                    }
                }
            }

            return false;
        }



        //returns max if there are multiple queens
        public static int QueenMaturityLevel(Map map)
        {
            int level = 0;


            List<Pawn> queens = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.TryGetComp<CompHQPresence>() != null).ToList();

            foreach(Pawn queen in queens)
            {
                CompHQPresence pres = queen.TryGetComp<CompHQPresence>();
                if (pres != null && pres.QueenMaturity > level)
                {
                    level = pres.QueenMaturity;
                }
            }

            return level;
        }


        public static Pawn TryGetQueen(Map map)
        {
            Pawn queen = null;

            List<Pawn> queens = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.TryGetComp<CompHQPresence>() != null).ToList();

            if (queens.Count > 0)
            {
                // there should never be more than one
                queen = queens.FirstOrDefault();
            }

            return queen;
        }




    }
}
