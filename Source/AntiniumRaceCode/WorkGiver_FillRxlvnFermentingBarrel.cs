using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode
{
    public class WorkGiver_FillRxlvnFermentingBarrel : WorkGiver_Scanner
    {
        private static string TemperatureTrans;

        private static string NoWortTrans;

        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(AntDefOf.Ant_RxlvnFermentingBarrel);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public static void ResetStaticData()
        {
            WorkGiver_FillRxlvnFermentingBarrel.TemperatureTrans = "BadTemperature".Translate().ToLower();
            WorkGiver_FillRxlvnFermentingBarrel.NoWortTrans = "NoWort".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_RxlvnFermentingBarrel building_RxlvnBarrel = t as Building_RxlvnFermentingBarrel;
            if (building_RxlvnBarrel == null || building_RxlvnBarrel.Fermented || building_RxlvnBarrel.SpaceLeftForMash <= 0)
            {
                return false;
            }
            float ambientTemperature = building_RxlvnBarrel.AmbientTemperature;
            CompProperties_TemperatureRuinable compProperties = building_RxlvnBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f || ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(WorkGiver_FillRxlvnFermentingBarrel.TemperatureTrans, null);
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                    {
                        return false;
                    }
                    if (this.FindWort(pawn, building_RxlvnBarrel) == null)
                    {
                        JobFailReason.Is(WorkGiver_FillRxlvnFermentingBarrel.NoWortTrans, null);
                        return false;
                    }
                    return !t.IsBurning();
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_RxlvnFermentingBarrel barrel = (Building_RxlvnFermentingBarrel)t;
            Thing t2 = this.FindWort(pawn, barrel);
            return new Job(AntDefOf.FillRxlvnFermentingBarrel, t, t2);
        }

        private Thing FindWort(Pawn pawn, Building_RxlvnFermentingBarrel barrel)
        {
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            IntVec3 position = pawn.Position;
            Map map = pawn.Map;
            ThingRequest thingReq = ThingRequest.ForDef(AntDefOf.Ant_RxlvnMash);
            PathEndMode peMode = PathEndMode.ClosestTouch;
            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
