using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntiniumRaceCode;

[StaticConstructorOnStartup]
public class Building_RxlvnFermentingBarrel : Building
{
    public const int MaxCapacity = 25;

    private const int BaseFermentationDuration = 360000;

    public const float MinIdealTemperature = 7f;

    private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

    private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

    private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

    private static readonly Material BarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private Material barFilledCachedMat;
    private int mashCount;

    private float progressInt;

    public float Progress
    {
        get => progressInt;
        set
        {
            if (value == progressInt)
            {
                return;
            }

            progressInt = value;
            barFilledCachedMat = null;
        }
    }

    private Material BarFilledMat
    {
        get
        {
            if (barFilledCachedMat == null)
            {
                barFilledCachedMat =
                    SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColor, BarFermentedColor,
                        Progress));
            }

            return barFilledCachedMat;
        }
    }

    public int SpaceLeftForMash
    {
        get
        {
            if (Fermented)
            {
                return 0;
            }

            return 25 - mashCount;
        }
    }

    private bool Empty => mashCount <= 0;

    public bool Fermented => !Empty && Progress >= 1f;

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

            return ambientTemperature < 7f
                ? GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, ambientTemperature)
                : 1f;
        }
    }

    private float ProgressPerTickAtCurrentTemp => 2.77777781E-06f * CurrentTempProgressSpeedFactor;

    private int EstimatedTicksLeft =>
        Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref mashCount, "mashCount");
        Scribe_Values.Look(ref progressInt, "progress");
    }

    public override void TickRare()
    {
        base.TickRare();
        if (!Empty)
        {
            Progress = Mathf.Min(Progress + (250f * ProgressPerTickAtCurrentTemp), 1f);
        }
    }

    public void AddMash(int count)
    {
        GetComp<CompTemperatureRuinable>().Reset();
        if (Fermented)
        {
            Log.Warning("Tried to add mash to a barrel full of rxlvn. Colonists should take the rxlvn first.");
            return;
        }

        var num = Mathf.Min(count, 25 - mashCount);
        if (num <= 0)
        {
            return;
        }

        Progress = GenMath.WeightedAverage(0f, num, Progress, mashCount);
        mashCount += num;
    }

    protected override void ReceiveCompSignal(string signal)
    {
        if (signal == "RuinedByTemperature")
        {
            Reset();
        }
    }

    private void Reset()
    {
        mashCount = 0;
        Progress = 0f;
    }

    public void AddMash(Thing mash)
    {
        var num = Mathf.Min(mash.stackCount, 25 - mashCount);
        if (num <= 0)
        {
            return;
        }

        AddMash(num);
        mash.SplitOff(num).Destroy();
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
        if (!Empty && !comp.Ruined)
        {
            stringBuilder.AppendLine(Fermented
                ? "ContainsRxlvn".Translate(mashCount, 25)
                : "ContainsMash".Translate(mashCount, 25));
        }

        if (!Empty)
        {
            if (Fermented)
            {
                stringBuilder.AppendLine("Fermented".Translate());
            }
            else
            {
                stringBuilder.AppendLine("FermentationProgress".Translate(Progress.ToStringPercent(),
                    EstimatedTicksLeft.ToStringTicksToPeriod()));
                if (CurrentTempProgressSpeedFactor != 1f)
                {
                    stringBuilder.AppendLine(
                        "FermentationBarrelOutOfIdealTemperature".Translate(CurrentTempProgressSpeedFactor
                            .ToStringPercent()));
                }
            }
        }

        stringBuilder.AppendLine("Temperature".Translate() + ": " + AmbientTemperature.ToStringTemperature("F0"));
        stringBuilder.AppendLine(
            $"{"IdealFermentingTemperature".Translate()}: {7f.ToStringTemperature("F0")} ~ {comp.Props.maxSafeTemperature.ToStringTemperature("F0")}");
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public Thing TakeOutRxlvn()
    {
        if (!Fermented)
        {
            Log.Warning("Tried to get rxlvn but it's not yet fermented.");
            return null;
        }

        var thing = ThingMaker.MakeThing(AntDefOf.Ant_Rxlvn);
        thing.stackCount = mashCount;
        Reset();
        return thing;
    }

    public override void Draw()
    {
        base.Draw();
        if (Empty)
        {
            return;
        }

        var drawPos = DrawPos;
        drawPos.y += 0.046875f;
        drawPos.z += 0.25f;
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = drawPos,
            size = BarSize,
            fillPercent = mashCount / 25f,
            filledMat = BarFilledMat,
            unfilledMat = BarUnfilledMat,
            margin = 0.1f,
            rotation = Rot4.North
        });
    }

    [DebuggerHidden]
    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }

        if (Prefs.DevMode && !Empty)
        {
            yield return new Command_Action
            {
                defaultLabel = "Debug: Set progress to 1",
                action = delegate { Progress = 1f; }
            };
        }
    }
}