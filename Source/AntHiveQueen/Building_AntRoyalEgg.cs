using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace AntiniumHiveQueen;

[StaticConstructorOnStartup]
public class Building_AntRoyalEgg : Building
{
    //private Material barFilledCachedMat;

    private const int BaseFermentationDuration = 360000;

    public const float MinIdealTemperature = 15f;

    private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

    private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

    private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

    private static readonly Material BarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private static readonly Material BarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f));

    private float progressInt;

    public float Progress
    {
        get => progressInt;
        set => progressInt = value;
        //this.barFilledCachedMat = null;
    }


    public bool Fermented => Progress >= 1f;

    private float CurrentTempProgressSpeedFactor
    {
        get
        {
            var compProperties = def.GetCompProperties<CompProperties_TemperatureRuinable>();
            var ambientTemperature = AmbientTemperature;
            if (ambientTemperature < compProperties.minSafeTemperature)
            {
                return 0.1f;
            }

            if (ambientTemperature < MinIdealTemperature)
            {
                return GenMath.LerpDouble(compProperties.minSafeTemperature, MinIdealTemperature, 0.1f, 1f,
                    ambientTemperature);
            }

            return 1f;
        }
    }

    private float ProgressPerTickAtCurrentTemp => 1.3E-06f * CurrentTempProgressSpeedFactor;

    private int EstimatedTicksLeft =>
        Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref progressInt, "progress");
    }

    public override void TickRare()
    {
        base.TickRare();

        Progress = Mathf.Min(Progress + (250f * ProgressPerTickAtCurrentTemp), 1f);

        if (Progress >= 1f)
        {
            //Log.Message("Royal egg hatching");
            Hatch();
        }
    }

    protected override void ReceiveCompSignal(string signal)
    {
        if (signal == "RuinedByTemperature")
        {
            //this.Reset();

            Destroy();
        }
    }


    public void Hatch()
    {
        try
        {
            var hatcherPawn = DefDatabase<PawnKindDef>.GetNamed("Ant_AntiniumQueen");
            var queenFactionDef = DefDatabase<FactionDef>.GetNamed("Ant_QueenFaction");
            var queenFaction =
                (from fac in Find.FactionManager.AllFactions where fac.def == queenFactionDef select fac).First();

            //PawnGenerationRequest request = new PawnGenerationRequest(hatcherPawn, queenFaction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, false, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, Gender.Female, null, null);

            var request = new PawnGenerationRequest(hatcherPawn, queenFaction, PawnGenerationContext.NonPlayer, -1,
                false, false, false, false, false, 1f, false, true, allowFood: true, allowAddictions: true,
                inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false,
                worldPawnFactionDoesntMatter: false,
                biocodeWeaponChance: 0f, biocodeApparelChance: 0f, extraPawnForExtraRelationChance: null,
                relationWithExtraPawnChanceFactor: 1f, validatorPreGear: null, validatorPostGear: null,
                forcedTraits: null, prohibitedTraits: null, minChanceToRedressWorldPawn: null, fixedBiologicalAge: null,
                fixedChronologicalAge: null, fixedGender: Gender.Female);

            var pawn = PawnGenerator.GeneratePawn(request);
            if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this))
            {
                if (pawn == null)
                {
                    return;
                }

                pawn.SetFaction(Faction.OfPlayer);

                var queenTrait = DefDatabase<TraitDef>.GetNamed("Ant_HiveQueenTrait");
                var traits = new List<Trait> { new Trait(queenTrait) };
                pawn.story.traits.allTraits = traits;

                pawn.health.AddHediff(AntHQDefOf.Ant_RoyalLarvaHediff);
                Find.LetterStack.ReceiveLetter("LetterLabelRoyalLarva".Translate(pawn),
                    "LetterRoyalLarva".Translate(pawn), LetterDefOf.PositiveEvent);

                FilthMaker.TryMakeFilth(Position, Map, ThingDefOf.Filth_AmnioticFluid);
            }
            else
            {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
            }
        }
        finally
        {
            Destroy();
        }
    }


    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        if (stringBuilder.Length != 0)
        {
            stringBuilder.AppendLine();
        }

        var comp = GetComp<CompTemperatureRuinable>();

        if (Fermented)
        {
            stringBuilder.AppendLine("ReadyToHatch".Translate());
        }
        else
        {
            stringBuilder.AppendLine("AntEggProgress".Translate(Progress.ToStringPercent(),
                EstimatedTicksLeft.ToStringTicksToPeriod()));
            if (CurrentTempProgressSpeedFactor != 1f)
            {
                stringBuilder.AppendLine(
                    "AntEggOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor.ToStringPercent()));
            }
        }

        stringBuilder.AppendLine("Temperature".Translate() + ": " + AmbientTemperature.ToStringTemperature("F0"));
        stringBuilder.AppendLine(
            $"{"AntEggIdealTemperature".Translate()}: {MinIdealTemperature.ToStringTemperature("F0")} ~ {comp.Props.maxSafeTemperature.ToStringTemperature("F0")}");
        return stringBuilder.ToString().TrimEndNewlines();
    }


    public override void Draw()
    {
        base.Draw();

        var drawPos = DrawPos;
        drawPos.y += 0.046875f;
        drawPos.z += 0.25f;
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = drawPos,
            size = BarSize,
            fillPercent = Progress,
            filledMat = BarFilledMat,
            unfilledMat = BarUnfilledMat,
            margin = 0.1f,
            rotation = Rot4.North
        });

        //base.Draw();
        //CompPowerBattery comp = base.GetComp<CompPowerBattery>();
        //GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
        //r.center = this.DrawPos + Vector3.up * 0.1f;
        //r.size = Building_Battery.BarSize;
        //r.fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
        //r.filledMat = Building_Battery.BatteryBarFilledMat;
        //r.unfilledMat = Building_Battery.BatteryBarUnfilledMat;
        //r.margin = 0.15f;
        //Rot4 rotation = base.Rotation;
        //rotation.Rotate(RotationDirection.Clockwise);
        //r.rotation = rotation;
        //GenDraw.DrawFillableBar(r);
    }

    [DebuggerHidden]
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }

        if (Prefs.DevMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "Debug: Set progress to 1",
                action = delegate { Progress = 1f; }
            };
        }
    }
}