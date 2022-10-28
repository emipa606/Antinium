using System;
using System.Linq;
using RimWorld;
using Verse;

namespace AntiniumRaceCode;

public class ThoughtWorker_BirdLoverSawBird : ThoughtWorker
{
    private const float radius = 12f;

    protected override ThoughtState CurrentStateInternal(Pawn pawn)
    {
        if (pawn == null)
        {
            return false;
        }

        if (!pawn.Spawned || !pawn.RaceProps.Humanlike)
        {
            return false;
        }

        if (pawn.story?.traits == null)
        {
            return false;
        }

        var birdLover = DefDatabase<TraitDef>.GetNamed("Ant_BirdLover");

        if (!pawn.story.traits.HasTrait(birdLover))
        {
            return false;
        }

        var mapPawns = pawn.Map.mapPawns.AllPawnsSpawned;
        var unused = mapPawns.Count;


        var birdPawns = mapPawns.Where(b => b.RaceProps?.body?.defName == "Bird").ToList();

        if (birdPawns.Count <= 0)
        {
            return false;
        }

        var birds = birdPawns.Count(c => c.Position.InHorDistOf(pawn.Position, radius));

        return birds > 0
            ? ThoughtState.ActiveAtStage(Math.Min(birds - 1, 4))
            :
            //Log.Message("NO BIRDS on map");
            false;
    }
}