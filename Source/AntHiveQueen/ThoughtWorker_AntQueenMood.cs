using RimWorld;
using Verse;

namespace AntiniumHiveQueen
{
    public class ThoughtWorker_AntQueenMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.kindDef.race.defName != "Ant_AntiniumRace" && p.GetStatValue(StatDefOf.PsychicSensitivity) < 1.3)
            {
                return false;
            }


            var relation = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");

            var directRelations = p.relations.DirectRelations;

            foreach (var directPawnRelation in directRelations)
            {
                var queen = directPawnRelation.otherPawn;

                if (queen.Dead || !queen.IsColonistPlayerControlled)
                {
                    continue;
                }

                if (directPawnRelation.def != relation)
                {
                    continue;
                }

                var mood = queen.needs.mood.CurLevel;

                if (mood < .1)
                {
                    return ThoughtState.ActiveAtStage(0);
                }

                if (mood < .25)
                {
                    return ThoughtState.ActiveAtStage(1);
                }

                if (mood <= .75)
                {
                    return ThoughtState.ActiveAtStage(2);
                }

                if (mood > .75)
                {
                    return ThoughtState.ActiveAtStage(3);
                }
            }

            return false;
        }
    }
}