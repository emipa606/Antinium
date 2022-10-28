using System.Collections.Generic;
using RimWorld;
using Verse;

namespace AntiniumHiveQueen;

public class IncidentWorker_RoyalEggSpawn : IncidentWorker
{
    protected override bool CanFireNowSub(IncidentParms parms)
    {
        var map = (Map)parms.target;

        if (map.mapTemperature.OutdoorTemp is <= -10 or >= 50)
        {
            return false;
        }

        if (map.mapPawns.FreeColonistsCount < 6)
        {
            return false;
        }

        if (map.mapPawns
                .FreeHumanlikesOfFaction(Faction.OfPlayer)
                .Count(p => p.kindDef?.race?.defName == "Ant_AntiniumRace") <
            3)
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
        var map = (Map)parms.target;
        var unused = SpawnRoyalEgg(map);


        return true;
    }


    private Thing SpawnRoyalEgg(Map map)
    {
        if (!TryFindCell(out var loc, map))
        {
            return null;
        }

        var thing = GenSpawn.Spawn(ThingMaker.MakeThing(AntHQDefOf.Ant_RoyalEgg), loc, map, WipeMode.FullRefund);

        //Find.LetterStack.ReceiveLetter("LetterLabelRoyalEgg".Translate(), "LetterRoyalEgg".Translate(), LetterDefOf.PositiveEvent);

        Find.LetterStack.ReceiveLetter("LetterLabelRoyalEgg".Translate(), "LetterRoyalEgg".Translate(),
            LetterDefOf.PositiveEvent, thing);


        return thing;
    }


    private static bool TryFindCell(out IntVec3 cell, Map map)
    {
        var locationCandidates = new List<LocationCandidate>();
        for (var i = 0; i < map.Size.z; i++)
        {
            for (var j = 0; j < map.Size.x; j++)
            {
                var cell2 = new IntVec3(j, 0, i);
                var scoreAt = GetScoreAt(cell2, map);
                if (scoreAt > 0f)
                {
                    locationCandidates.Add(new LocationCandidate(cell2, scoreAt));
                }
            }
        }

        if (!locationCandidates.TryRandomElementByWeight(x => x.score, out var locationCandidate))
        {
            cell = IntVec3.Invalid;
            return false;
        }

        cell = CellFinder.FindNoWipeSpawnLocNear(locationCandidate.cell, map, ThingDefOf.Hive, Rot4.North, 2,
            x => GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.Hive) == null &&
                 x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
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

        var temperature = cell.GetTemperature(map);
        if (temperature < -20f)
        {
            return 0f;
        }

        var score = 1f;

        if (!cell.Roofed(map))
        {
            score *= .03f;
        }
        else if (!cell.GetRoof(map).isThickRoof)
        {
            score *= .5f;
        }

        var mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
        if (mountainousnessScoreAt < 0.17f)
        {
            score *= .3f;
        }

        return score;
    }


    private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
    {
        var num = 0f;
        var num2 = 0;
        for (var i = 0; i < 700; i += 10)
        {
            var c = cell + GenRadial.RadialPattern[i];
            if (!c.InBounds(map))
            {
                continue;
            }

            var edifice = c.GetEdifice(map);
            if (edifice != null && edifice.def.category == ThingCategory.Building &&
                edifice.def.building.isNaturalRock)
            {
                num += 1f;
            }
            else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
            {
                num += 0.5f;
            }

            num2++;
        }

        return num / num2;
    }

    private struct LocationCandidate
    {
        public readonly IntVec3 cell;

        public readonly float score;

        public LocationCandidate(IntVec3 cell, float score)
        {
            this.cell = cell;
            this.score = score;
        }
    }
}