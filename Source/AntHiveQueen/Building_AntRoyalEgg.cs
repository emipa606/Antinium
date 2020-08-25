using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using System.Diagnostics;
using RimWorld.Planet;

namespace AntiniumHiveQueen
{

    [StaticConstructorOnStartup]
    public class Building_AntRoyalEgg : Building
    {

        private float progressInt;

        //private Material barFilledCachedMat;

        private const int BaseFermentationDuration = 360000;

        public const float MinIdealTemperature = 15f;

        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

        private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

        private static readonly Material BarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
                //this.barFilledCachedMat = null;
            }
        }

        private static readonly Material BarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.9f, 0.85f, 0.2f), false);



        public bool Fermented
        {
            get
            {
                return this.Progress >= 1f;
            }
        }

        private float CurrentTempProgressSpeedFactor
        {
            get
            {
                CompProperties_TemperatureRuinable compProperties = this.def.GetCompProperties<CompProperties_TemperatureRuinable>();
                float ambientTemperature = base.AmbientTemperature;
                if (ambientTemperature < compProperties.minSafeTemperature)
                {
                    return 0.1f;
                }
                if (ambientTemperature < MinIdealTemperature)
                {
                    return GenMath.LerpDouble(compProperties.minSafeTemperature, MinIdealTemperature, 0.1f, 1f, ambientTemperature);
                }
                return 1f;
            }
        }

        private float ProgressPerTickAtCurrentTemp
        {
            get
            {
                return 1.3E-06f * this.CurrentTempProgressSpeedFactor;
            }
        }

        private int EstimatedTicksLeft
        {
            get
            {
                return Mathf.Max(Mathf.RoundToInt((1f - this.Progress) / this.ProgressPerTickAtCurrentTemp), 0);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }

        public override void TickRare()
        {
            base.TickRare();

            this.Progress = Mathf.Min(this.Progress + 250f * this.ProgressPerTickAtCurrentTemp, 1f);

            if (this.Progress >= 1f)
            {
                //Log.Message("Royal egg hatching");
                this.Hatch();
            }

        }

        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                //this.Reset();

                this.Destroy();

            }
        }



        public void Hatch()
        {
            try
            {
                PawnKindDef hatcherPawn = DefDatabase<PawnKindDef>.GetNamed("Ant_AntiniumQueen");
                FactionDef queenFactionDef = DefDatabase<FactionDef>.GetNamed("Ant_QueenFaction");
                Faction queenFaction = (from fac in Find.FactionManager.AllFactions where fac.def == queenFactionDef select fac).First();

                //PawnGenerationRequest request = new PawnGenerationRequest(hatcherPawn, queenFaction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, false, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, Gender.Female, null, null);

                PawnGenerationRequest request = new PawnGenerationRequest(hatcherPawn, queenFaction, PawnGenerationContext.NonPlayer, -1, false, true, false, false, false, false, 1f, false, true, true, true, false, false, false, false, 0f, null, 1f, null, null, null, null, null, null, null, Gender.Female, null, null, null, null);

                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (PawnUtility.TrySpawnHatchedOrBornPawn(pawn, this))
                {
                    if (pawn != null)
                    {

                        pawn.SetFaction(Faction.OfPlayer);

                        TraitDef queenTrait = DefDatabase<TraitDef>.GetNamed("Ant_HiveQueenTrait");
                        List<Trait> traits = new List<Trait> { new Trait(queenTrait) };
                        pawn.story.traits.allTraits = traits;

                        pawn.health.AddHediff(AntHQDefOf.Ant_RoyalLarvaHediff);
                        Find.LetterStack.ReceiveLetter("LetterLabelRoyalLarva".Translate(pawn), "LetterRoyalLarva".Translate(pawn), LetterDefOf.PositiveEvent);

                        FilthMaker.TryMakeFilth(this.Position, this.Map, ThingDefOf.Filth_AmnioticFluid, 1);
                    }

                }
                else
                {
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                }

            }
            finally
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }






        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            CompTemperatureRuinable comp = base.GetComp<CompTemperatureRuinable>();

            if (this.Fermented)
            {
                stringBuilder.AppendLine("ReadyToHatch".Translate());
            }
            else
            {
                stringBuilder.AppendLine("AntEggProgress".Translate(this.Progress.ToStringPercent(), this.EstimatedTicksLeft.ToStringTicksToPeriod()));
                if (this.CurrentTempProgressSpeedFactor != 1f)
                {
                    stringBuilder.AppendLine("AntEggOutOfIdealTemperature".Translate(this.CurrentTempProgressSpeedFactor.ToStringPercent()));
                }
            }

            stringBuilder.AppendLine("Temperature".Translate() + ": " + base.AmbientTemperature.ToStringTemperature("F0"));
            stringBuilder.AppendLine(string.Concat(new string[]
            {
                "AntEggIdealTemperature".Translate(),
                ": ",
                MinIdealTemperature.ToStringTemperature("F0"),
                " ~ ",
                comp.Props.maxSafeTemperature.ToStringTemperature("F0")
            }));
            return stringBuilder.ToString().TrimEndNewlines();
        }


        public override void Draw()
        {
            base.Draw();

            Vector3 drawPos = this.DrawPos;
            drawPos.y += 0.046875f;
            drawPos.z += 0.25f;
            GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
            {
                center = drawPos,
                size = Building_AntRoyalEgg.BarSize,
                fillPercent = (float)this.Progress,
                filledMat = Building_AntRoyalEgg.BarFilledMat,
                unfilledMat = Building_AntRoyalEgg.BarUnfilledMat,
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
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate
                    {
                        this.Progress = 1f;
                    }
                };
            }
        }
    }
}

