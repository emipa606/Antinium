using RimWorld;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode
{
    public class WorkGiver_TakeRxlvnOutOfFermentingBarrel : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(AntDefOf.Ant_RxlvnFermentingBarrel);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_RxlvnFermentingBarrel building_RxlvnBarrel) || !building_RxlvnBarrel.Fermented)
            {
                return false;
            }

            if (t.IsBurning())
            {
                return false;
            }

            if (t.IsForbidden(pawn))
            {
                return false;
            }

            LocalTargetInfo target = t;
            if (pawn.CanReserve(target, 1, -1, null, forced))
            {
                return true;
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(AntDefOf.TakeRxlvnOutOfFermentingBarrel, t);
        }
    }
}