﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;



namespace AntiniumHiveQueen
{
    class ThoughtWorker_QueenHealthEmergency : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            //if (!(p.kindDef.race.defName == "Ant_AntiniumRace") && p.GetStatValue(StatDefOf.PsychicSensitivity, true) < 1.3)
            //{
            //    return false;
            //}


            PawnRelationDef relation = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");

            List<DirectPawnRelation> directRelations = p.relations.DirectRelations;

            Pawn queen;

            for (int i = 0; i < directRelations.Count; i++)
            {
                queen = directRelations[i].otherPawn;

                if (!queen.Dead && queen.IsColonistPlayerControlled)
                {

                    //if (directRelations[i].def == relation)
                    //{

                    for (int j = 0; j < queen.health.hediffSet.hediffs.Count; j++)
                    {
                        Hediff diff = queen.health.hediffSet.hediffs[j];
                        if (diff.CurStage != null && diff.CurStage.lifeThreatening && !diff.FullyImmune())
                        {
                            return true;
                        }
                    }


                    //}
                }
            }

            return false;

        }
    }
}