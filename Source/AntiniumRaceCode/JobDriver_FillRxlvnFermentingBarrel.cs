using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode;

public class JobDriver_FillRxlvnFermentingBarrel : JobDriver
{
    private const TargetIndex BarrelInd = TargetIndex.A;

    private const TargetIndex WortInd = TargetIndex.B;

    private const int Duration = 200;

    protected Building_RxlvnFermentingBarrel Barrel =>
        (Building_RxlvnFermentingBarrel)job.GetTarget(TargetIndex.A).Thing;

    protected Thing Ant_RxlvnMash => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var pawn1 = pawn;
        LocalTargetInfo target = Barrel;
        var job1 = job;
        bool arg_58_0;
        if (pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed))
        {
            pawn1 = pawn;
            target = Ant_RxlvnMash;
            job1 = job;
            arg_58_0 = pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
        }
        else
        {
            arg_58_0 = false;
        }

        return arg_58_0;
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        AddEndCondition(() => Barrel.SpaceLeftForMash > 0 ? JobCondition.Ongoing : JobCondition.Succeeded);
        yield return Toils_General.DoAtomic(delegate { job.count = Barrel.SpaceLeftForMash; });
        var reserveWort = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveWort;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveWort, TargetIndex.B, TargetIndex.None, true);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate { Barrel.AddMash(Ant_RxlvnMash); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}