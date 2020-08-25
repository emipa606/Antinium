using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace AntiniumHiveQueen
{
    public class CompHQPresence : ThingComp
    {

        public int QueenMaturity
        {
            get
            {
                int maturity = 0;

                // age
                int age = 0;
                Pawn queen = this.parent as Pawn;
                age = queen.ageTracker.AgeBiologicalYears;

                if (age < 14)
                {
                    maturity = 0;
                }
                else if (age < 20)
                {
                    maturity = 1;
                }
                else
                {
                    maturity = 2;
                }

                // time at the colony
                long time = queen.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal);

                if (time >= 3600000 * 2)
                {
                    maturity++;
                }

                if (time >= 3600000 * 3)
                {
                    maturity++;
                }

                if (time >= 3600000 * 4)
                {
                    maturity++;
                }

                if (time >= 3600000 * 5)
                {
                    maturity++;
                }
                return maturity;
            }
        }


        public int QueenStrength
        {
            get
            {
                return this.queenStrength;
            }
        }

        private int queenStrength
        {
            get
            {
                int strength = this.QueenMaturity;

                Pawn pawn = this.parent as Pawn;

                // health
                if (pawn.Downed || pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < .6)
                {
                    strength /= 2;
                }


                if (pawn.health.hediffSet.hediffs.Any(h => h.CurStage != null && h.CurStage.lifeThreatening && !h.FullyImmune()))
                {
                    strength--;
                }

                //happiness
                if(pawn.needs.mood.CurLevel > .85)
                {
                    strength++;
                }

                return strength;
            }
        }

        public bool Active { get { return this.active; } }

        private bool active
        {
            get
            {
                Pawn pawn = this.parent as Pawn;
                TraitDef queenTrait = DefDatabase<TraitDef>.GetNamed("Ant_HiveQueenTrait");
                return pawn.story.traits.HasTrait(queenTrait);
            }
        }


        public override void CompTick()
        {
            if (this.active)
            {
                Pawn pawn = this.parent as Pawn;
                if (pawn != null)
                {
                    if (Find.TickManager.TicksGame % 3803 == 0)
                    {
                        ApplyQueenRelation(pawn);

                        ApplyQueenHediff(pawn.Map);

                    }
                }
            }
        }


        private static void ApplyQueenRelation(Pawn queen)
        {
            PawnRelationDef relation = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");
            // all free colonists on map
            List<Pawn> colonists = new List<Pawn>();
            colonists = queen.Map.mapPawns.FreeColonists.ToList();
            foreach (Pawn c in colonists.Where(p => p.RaceProps.Humanlike && p != queen))
            {
                if (c.relations.GetDirectRelation(relation, queen) == null)
                {
                    // remove any old Q relations
                    List<DirectPawnRelation> others = new List<DirectPawnRelation>();
                    others = c.relations.DirectRelations.Where(r => r.def == relation).ToList();

                    if (others.Count != 0)
                    {
                        foreach (DirectPawnRelation r in others)
                        {
                            c.relations.RemoveDirectRelation(r);
                        }
                    }
                    // add new Q relation
                    c.relations.AddDirectRelation(relation, queen);
                }

            }

        }


        private void ApplyQueenHediff(Map map)
        {
            List<Pawn> colonists = map.mapPawns.FreeColonists.ToList();
            Pawn queen = this.parent as Pawn;

            foreach (Pawn c in colonists.Where(p => p != queen))
            {
                //Pawn pawn = colonists[i];
                if (c.RaceProps.Humanlike)
                {
                    SetQueenHediffSeverity(c);
                }
            }
            //for (int i = 0; i < colonists.Count; i++)
            //{
            //    Pawn pawn = colonists[i];
            //    if (pawn.RaceProps.Humanlike)
            //    {
            //        SetQueenHediffSeverity(pawn);
            //    }
            //}
        }


        private void SetQueenHediffSeverity(Pawn pawn)
        {
            float severity = 0f;

            int score = -1;

            score = HiveQueenUtility.GetPawnHQScore(pawn);

            if (score < 0)
            {
                score *= 2;
            }

            score += QueenStrength;

            if(score > 0)
            {
                severity = score * .1f;
                severity = Math.Min(severity, 1f);
            }
            else
            {
                severity = 0f;
            }

            // do hediff
            Hediff olddiff = pawn.health.hediffSet.GetFirstHediffOfDef(AntHQDefOf.Ant_HiveQueenInspHediff, false);
            if (olddiff != null)
            {
                pawn.health.RemoveHediff(olddiff);
            }

            if (severity > 0f)
            {
                Hediff hediff = HediffMaker.MakeHediff(AntHQDefOf.Ant_HiveQueenInspHediff, pawn, null);
                hediff.Severity = severity;
                pawn.health.AddHediff(hediff, null, null, null);
            }


        }

    }
}