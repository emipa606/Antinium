using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntiniumHiveQueen
{
    public class IncidentWorker_RoyalEggSpawn : IncidentWorker
    {

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map map = (Map)parms.target;

            if (map.mapTemperature.OutdoorTemp <= -10 || map.mapTemperature.OutdoorTemp >= 50)
            {
                return false;
            }

            if (map.mapPawns.FreeColonistsCount < 6)
            {
                return false;
            }
            if (map.mapPawns.FreeHumanlikesOfFaction(Faction.OfPlayer).Where(p => p.kindDef?.race?.defName == "Ant_AntiniumRace").Count() < 3)
            {
                return false;
            }
            if (HiveQueenUtility.QueenExists())
            {
                return false;
            }

            return true;
        }


        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Thing t = this.SpawnRoyalEgg(map);


            return true;
        }


        private Thing SpawnRoyalEgg(Map map)
        {

            IntVec3 loc;
            if (!TryFindCell(out loc, map))
            {
                return null;
            }

            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(AntHQDefOf.Ant_RoyalEgg, null), loc, map, WipeMode.FullRefund);

            //Find.LetterStack.ReceiveLetter("LetterLabelRoyalEgg".Translate(), "LetterRoyalEgg".Translate(), LetterDefOf.PositiveEvent);

            Find.LetterStack.ReceiveLetter("LetterLabelRoyalEgg".Translate(), "LetterRoyalEgg".Translate(), LetterDefOf.PositiveEvent, thing, null, null);

            
            return thing;
        }

        private struct LocationCandidate
        {
            public IntVec3 cell;

            public float score;

            public LocationCandidate(IntVec3 cell, float score)
            {
                this.cell = cell;
                this.score = score;
            }
        }


        private static bool TryFindCell(out IntVec3 cell, Map map)
        {

            List<LocationCandidate> locationCandidates = new List<LocationCandidate>();
            for (int i = 0; i < map.Size.z; i++)
            {
                for (int j = 0; j < map.Size.x; j++)
                {
                    IntVec3 cell2 = new IntVec3(j, 0, i);
                    float scoreAt = GetScoreAt(cell2, map);
                    if (scoreAt > 0f)
                    {
                        locationCandidates.Add(new LocationCandidate(cell2, scoreAt));
                    }
                }
            }

            LocationCandidate locationCandidate;

            if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out locationCandidate))
            {
                cell = IntVec3.Invalid;
                return false;
            }
            cell = CellFinder.FindNoWipeSpawnLocNear(locationCandidate.cell, map, ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
            return true;
        }


        private static float GetScoreAt(IntVec3 cell, Map map)
        {

            if (!cell.Walkable(map))
            {
                return 0f;
            }
            if (cell.Fogged(map))
            {
                return 0f;
            }
            float temperature = cell.GetTemperature(map);
            if (temperature < -20f)
            {
                return 0f;
            }
            float score = 1f;

            if (!cell.Roofed(map))
            {
                score *= .03f;
            }
            else if (!cell.GetRoof(map).isThickRoof)
            {
                score *= .5f;
            }

            float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
            if (mountainousnessScoreAt < 0.17f)
            {
                score *= .3f;
            }

            return score;

        }



        private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
        {
            float num = 0f;
            int num2 = 0;
            for (int i = 0; i < 700; i += 10)
            {
                IntVec3 c = cell + GenRadial.RadialPattern[i];
                if (c.InBounds(map))
                {
                    Building edifice = c.GetEdifice(map);
                    if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
                    {
                        num += 1f;
                    }
                    else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
                    {
                        num += 0.5f;
                    }
                    num2++;
                }
            }
            return num / (float)num2;
        }

    }
}
