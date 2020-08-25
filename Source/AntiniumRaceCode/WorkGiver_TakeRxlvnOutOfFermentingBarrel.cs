using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;


namespace AntiniumRaceCode
{
    public class WorkGiver_TakeRxlvnOutOfFermentingBarrel : WorkGiver_Scanner
    {
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

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_RxlvnFermentingBarrel building_RxlvnBarrel = t as Building_RxlvnFermentingBarrel;
            if (building_RxlvnBarrel == null || !building_RxlvnBarrel.Fermented)
            {
                return false;
            }
            if (t.IsBurning())
            {
                return false;
            }
            if (!t.IsForbidden(pawn))
            {
                LocalTargetInfo target = t;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    return true;
                }
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(AntDefOf.TakeRxlvnOutOfFermentingBarrel, t);
        }
    }
}
