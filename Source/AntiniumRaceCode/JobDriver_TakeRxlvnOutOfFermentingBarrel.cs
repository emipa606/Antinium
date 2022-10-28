using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode;

public class JobDriver_TakeRxlvnOutOfFermentingBarrel : JobDriver
{
    private const TargetIndex BarrelInd = TargetIndex.A;

    private const TargetIndex BeerToHaulInd = TargetIndex.B;

    private const TargetIndex StorageCellInd = TargetIndex.C;

    private const int Duration = 200;

    protected Building_RxlvnFermentingBarrel Barrel =>
        (Building_RxlvnFermentingBarrel)job.GetTarget(TargetIndex.A).Thing;

    protected Thing Beer => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var pawn1 = pawn;
        LocalTargetInfo target = Barrel;
        var job1 = job;
        return pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).FailOn(() => !Barrel.Fermented)
            .WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate
            {
                var thing = Barrel.TakeOutRxlvn();
                GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near);
                var currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, currentPriority, pawn.Faction,
                        out var c))
                {
                    job.SetTarget(TargetIndex.C, c);
                    job.SetTarget(TargetIndex.B, thing);
                    job.count = thing.stackCount;
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        var carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
        yield return carryToCell;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);
    }
}