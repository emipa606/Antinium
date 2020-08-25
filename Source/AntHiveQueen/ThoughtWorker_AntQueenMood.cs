using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;


namespace AntiniumHiveQueen
{
    public class ThoughtWorker_AntQueenMood : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            if(!(p.kindDef.race.defName == "Ant_AntiniumRace") && p.GetStatValue(StatDefOf.PsychicSensitivity, true) < 1.3)
            {
                return false;
            }


            PawnRelationDef relation = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");

            List<DirectPawnRelation> directRelations = p.relations.DirectRelations;

            Pawn queen;

            for (int i = 0; i < directRelations.Count; i++)
            {
                queen = directRelations[i].otherPawn;

                if (!queen.Dead && queen.IsColonistPlayerControlled)
                {

                    if (directRelations[i].def == relation)
                    {

                        float mood = queen.needs.mood.CurLevel;

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
                }
            }

            return false;

        }
    }
}
