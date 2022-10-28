using RimWorld;
using Verse;

namespace AntiniumHiveQueen;

internal class ThoughtWorker_QueenHealthEmergency : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        //if (!(p.kindDef.race.defName == "Ant_AntiniumRace") && p.GetStatValue(StatDefOf.PsychicSensitivity, true) < 1.3)
        //{
        //    return false;
        //}


        var unused = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");

        var directRelations = p.relations.DirectRelations;

        foreach (var directPawnRelation in directRelations)
        {
            var queen = directPawnRelation.otherPawn;

            if (queen.Dead || !queen.IsColonistPlayerControlled)
            {
                continue;
            }
            //if (directRelations[i].def == relation)
            //{

            foreach (var diff in queen.health.hediffSet.hediffs)
            {
                if (diff.CurStage is { lifeThreatening: true } && !diff.FullyImmune())
                {
                    return true;
                }
            }


            //}
        }

        return false;
    }
}