using System;
using System.Linq;
using RimWorld;
using Verse;

namespace AntiniumHiveQueen
{
    public static class HiveQueenUtility
    {
        // general sensitivity to queen effects
        public static int GetPawnHQScore(Pawn pawn, bool antsOnly = false, bool reqForAnts = true)
        {
            var factor = -1;

            if (antsOnly && pawn.kindDef.race.defName != "Ant_AntiniumRace")
            {
                return -1;
            }

            // default increase for ants
            if (pawn.kindDef.race.defName == "Ant_AntiniumRace")
            {
                factor = 2;
            }

            // psychic sensitivity
            var hyper = pawn.GetStatValue(StatDefOf.PsychicSensitivity);


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
            var list = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(p => p.RaceProps.Humanlike).ToList();

            foreach (var pawn in list)
            {
                var comp = pawn.TryGetComp<CompHQPresence>();
                if (comp == null)
                {
                    continue;
                }

                if (comp.Active)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool QueenExists()
        {
            var list = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction.ToList();

            foreach (var pawn in list)
            {
                var comp = pawn.TryGetComp<CompHQPresence>();
                if (comp == null)
                {
                    continue;
                }

                if (comp.Active)
                {
                    return true;
                }
            }

            return false;
        }


        //returns max if there are multiple queens
        public static int QueenMaturityLevel(Map map)
        {
            var level = 0;


            var queens = map.mapPawns.PawnsInFaction(Faction.OfPlayer)
                .Where(p => p.TryGetComp<CompHQPresence>() != null).ToList();

            foreach (var queen in queens)
            {
                var pres = queen.TryGetComp<CompHQPresence>();
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

            var queens = map.mapPawns.PawnsInFaction(Faction.OfPlayer)
                .Where(p => p.TryGetComp<CompHQPresence>() != null).ToList();

            if (queens.Count > 0)
            {
                // there should never be more than one
                queen = queens.FirstOrDefault();
            }

            return queen;
        }
    }
}