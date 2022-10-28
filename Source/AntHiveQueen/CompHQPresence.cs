using System;
using System.Linq;
using RimWorld;
using Verse;

namespace AntiniumHiveQueen;

public class CompHQPresence : ThingComp
{
    public int QueenMaturity
    {
        get
        {
            var maturity = 0;

            // age
            var queen = parent as Pawn;
            if (queen != null)
            {
                var age = queen.ageTracker.AgeBiologicalYears;

                switch (age)
                {
                    case < 14:
                        maturity = 0;
                        break;
                    case < 20:
                        maturity = 1;
                        break;
                    default:
                        maturity = 2;
                        break;
                }
            }

            // time at the colony
            if (queen == null)
            {
                return maturity;
            }

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


    public int QueenStrength => queenStrength;

    private int queenStrength
    {
        get
        {
            var strength = QueenMaturity;

            var pawn = parent as Pawn;

            // health
            if (pawn != null &&
                (pawn.Downed || pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) < .6))
            {
                strength /= 2;
            }


            if (pawn != null && pawn.health.hediffSet.hediffs.Any(h =>
                    h.CurStage is { lifeThreatening: true } && !h.FullyImmune()))
            {
                strength--;
            }

            //happiness
            if (pawn != null && pawn.needs.mood.CurLevel > .85)
            {
                strength++;
            }

            return strength;
        }
    }

    public bool Active => active;

    private bool active
    {
        get
        {
            var pawn = parent as Pawn;
            var queenTrait = DefDatabase<TraitDef>.GetNamed("Ant_HiveQueenTrait");
            return pawn != null && pawn.story.traits.HasTrait(queenTrait);
        }
    }


    public override void CompTick()
    {
        if (!active)
        {
            return;
        }

        if (parent is not Pawn pawn)
        {
            return;
        }

        if (Find.TickManager.TicksGame % 3803 != 0)
        {
            return;
        }

        ApplyQueenRelation(pawn);

        ApplyQueenHediff(pawn.Map);
    }


    private static void ApplyQueenRelation(Pawn queen)
    {
        var relation = DefDatabase<PawnRelationDef>.GetNamed("Ant_QueenRelation");
        // all free colonists on map
        var colonists = queen.Map.mapPawns.FreeColonists.ToList();
        foreach (var c in colonists.Where(p => p.RaceProps.Humanlike && p != queen))
        {
            if (c.relations.GetDirectRelation(relation, queen) != null)
            {
                continue;
            }

            // remove any old Q relations
            var others = c.relations.DirectRelations.Where(r => r.def == relation).ToList();

            if (others.Count != 0)
            {
                foreach (var r in others)
                {
                    c.relations.RemoveDirectRelation(r);
                }
            }

            // add new Q relation
            c.relations.AddDirectRelation(relation, queen);
        }
    }


    private void ApplyQueenHediff(Map map)
    {
        var colonists = map.mapPawns.FreeColonists.ToList();
        var queen = parent as Pawn;

        foreach (var c in colonists.Where(p => p != queen))
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
        float severity;

        var score = HiveQueenUtility.GetPawnHQScore(pawn);

        if (score < 0)
        {
            score *= 2;
        }

        score += QueenStrength;

        if (score > 0)
        {
            severity = score * .1f;
            severity = Math.Min(severity, 1f);
        }
        else
        {
            severity = 0f;
        }

        // do hediff
        var olddiff = pawn.health.hediffSet.GetFirstHediffOfDef(AntHQDefOf.Ant_HiveQueenInspHediff);
        if (olddiff != null)
        {
            pawn.health.RemoveHediff(olddiff);
        }

        if (!(severity > 0f))
        {
            return;
        }

        var hediff = HediffMaker.MakeHediff(AntHQDefOf.Ant_HiveQueenInspHediff, pawn);
        hediff.Severity = severity;
        pawn.health.AddHediff(hediff);
    }
}