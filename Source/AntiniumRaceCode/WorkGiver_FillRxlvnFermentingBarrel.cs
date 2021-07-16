using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode
{
    public class WorkGiver_FillRxlvnFermentingBarrel : WorkGiver_Scanner
    {
        private static string TemperatureTrans;

        private static string NoWortTrans;

        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(AntDefOf.Ant_RxlvnFermentingBarrel);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public static void ResetStaticData()
        {
            TemperatureTrans = "BadTemperature".Translate().ToLower();
            NoWortTrans = "NoWort".Translate();
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_RxlvnFermentingBarrel building_RxlvnBarrel) || building_RxlvnBarrel.Fermented ||
                building_RxlvnBarrel.SpaceLeftForMash <= 0)
            {
                return false;
            }

            var ambientTemperature = building_RxlvnBarrel.AmbientTemperature;
            var compProperties = building_RxlvnBarrel.def.GetCompProperties<CompProperties_TemperatureRuinable>();
            if (ambientTemperature < compProperties.minSafeTemperature + 2f ||
                ambientTemperature > compProperties.maxSafeTemperature - 2f)
            {
                JobFailReason.Is(TemperatureTrans);
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }

            LocalTargetInfo target = t;
            if (!pawn.CanReserve(target, 1, -1, null, forced))
            {
                return false;
            }

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }

            if (FindWort(pawn) != null)
            {
                return !t.IsBurning();
            }

            JobFailReason.Is(NoWortTrans);
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var unused = (Building_RxlvnFermentingBarrel) t;
            var t2 = FindWort(pawn);
            return new Job(AntDefOf.FillRxlvnFermentingBarrel, t, t2);
        }

        private Thing FindWort(Pawn pawn)
        {
            bool Predicate(Thing x)
            {
                return !x.IsForbidden(pawn) && pawn.CanReserve(x);
            }

            var position = pawn.Position;
            var map = pawn.Map;
            var thingReq = ThingRequest.ForDef(AntDefOf.Ant_RxlvnMash);
            var peMode = PathEndMode.ClosestTouch;
            var traverseParams = TraverseParms.For(pawn);
            var validator = (Predicate<Thing>) Predicate;
            return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, 9999f, validator);
        }
    }
}