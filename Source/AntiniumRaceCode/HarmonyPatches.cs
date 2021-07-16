using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AntiniumRaceCode
{
    [StaticConstructorOnStartup]
    internal static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            var harmony = new Harmony("rimworld.antinium");

            // find the AddFoodPoisoningHediff method of the class RimWorld.FoodUtility
            var targetmethod = AccessTools.Method(typeof(FoodUtility), "AddFoodPoisoningHediff");

            // find the static method to call before (i.e. Prefix) the targetmethod
            var prefixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("AddFoodPoisoningHediff_Prefix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch(targetmethod, prefixmethod);

            // Bird lover eats bird
            targetmethod = AccessTools.Method(typeof(FoodUtility), "AddIngestThoughtsFromIngredient");
            prefixmethod =
                new HarmonyMethod(typeof(HarmonyPatches).GetMethod("AddIngestThoughtsFromIngredient_Prefix"));
            harmony.Patch(targetmethod, prefixmethod);

            //// Ant eats insect meat
            //targetmethod = AccessTools.Method(typeof(FoodUtility), "ThoughtsFromIngesting");
            //var postfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("ThoughtsFromIngesting_Postfix"));
            //harmony.Patch(targetmethod, null, postfixmethod);

            // Aberration
            targetmethod = AccessTools.Method(typeof(MentalBreaker), "TryDoRandomMoodCausedMentalBreak");
            var postfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("MentalBreak_Abberation_Postfix"));
            harmony.Patch(targetmethod, null, postfixmethod);

            // Drug tolerance
            targetmethod = AccessTools.Method(typeof(AddictionUtility), "ModifyChemicalEffectForToleranceAndBodySize");
            postfixmethod =
                new HarmonyMethod(
                    typeof(HarmonyPatches).GetMethod("ModifyChemicalEffectForToleranceAndBodySize_Postfix"));
            harmony.Patch(targetmethod, null, postfixmethod);

            #region HAR framework patches

            //Posture
            targetmethod = AccessTools.Method(typeof(AlienRace.HarmonyPatches), "PostureTweak");
            postfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("PostureTweak_Postfix"));
            harmony.Patch(targetmethod, null, postfixmethod);

            #endregion
        }


        #region HAR framework patches

        //Posture
        public static void PostureTweak_Postfix(Pawn pawn, ref PawnPosture __result)
        {
            var antBeds = new List<string> {"AntSleepingSpot", "AntSleepingAlcove", "AntFluffFortress"};

            if (pawn.kindDef?.race?.defName != "Ant_AntiniumRace")
            {
                return;
            }

            if (antBeds.Contains(pawn.CurrentBed()?.def.defName))
            {
                __result = PawnPosture.Standing;
            }
        }

        #endregion

        #region Rimworld patches

        // This method is now always called right before RimWorld.FoodUtility.AddFoodPoisoningHediff.
        public static bool AddFoodPoisoningHediff_Prefix(Pawn pawn)
        {
            if (pawn.kindDef.race.defName == "Ant_AntiniumRace")
            {
                return false;
            }

            return true;
        }


        public static void AddIngestThoughtsFromIngredient_Prefix(ThingDef ingredient, Pawn ingester,
            ref List<ThoughtDef> ingestThoughts)
        {
            var birdLover = DefDatabase<TraitDef>.GetNamed("Ant_BirdLover");

            if (ingester?.story?.traits == null)
            {
                return;
            }

            if (!ingester.story.traits.HasTrait(birdLover))
            {
                return;
            }

            //if (ingredient.ingestible.sourceDef.race.body.defName == "Bird" || ingredient.ingestible.sourceDef.race.leatherDef.defName == "Leather_Bird")
            //if (ingredient.ingestible.sourceDef.race.body.defName == "Bird" )
            if (ingredient?.ingestible?.sourceDef?.race?.body?.defName != "Bird")
            {
                return;
            }

            var ateBird = DefDatabase<ThoughtDef>.GetNamed("Ant_AteBirdMeatAsIngredient");
            ingestThoughts.Add(ateBird);
            //ingestThoughts.Add(AntDefOf.Ant_AteBirdMeatAsIngredient);
        }


        //// to fix insect meat food priority
        //public static void ThoughtsFromIngesting_Postfix(Pawn ingester, ref List<FoodUtility.ThoughtFromIngesting> __result)
        //{
        //    if (ingester.kindDef.race.defName != "Ant_AntiniumRace")
        //    {
        //        return;
        //    }

        //    // AteInsectMeatAsIngredient
        //    if (__result.Contains(AteInsectMeatAsIngredient))
        //    {
        //        __result.Remove(ThoughtDefOf.AteInsectMeatAsIngredient);
        //        var ateInsectIngredient = DefDatabase<ThoughtDef>.GetNamed("Ant_AteInsectMeatAsIngredient");
        //        __result.Add(ateInsectIngredient);
        //    }

        //    // AteInsectMeatDirect
        //    else if (__result.Contains(ThoughtDefOf.AteInsectMeatDirect))
        //    {
        //        __result.Remove(ThoughtDefOf.AteInsectMeatDirect);
        //        var ateInsectDirect = DefDatabase<ThoughtDef>.GetNamed("Ant_AteInsectMeatDirect");
        //        __result.Add(ateInsectDirect);
        //    }
        //}


        public static void MentalBreak_Abberation_Postfix(MentalBreaker __instance, ref bool __result)
        {
            // Log.Message("aberration method fired");
            int.TryParse(
                "" + (byte) Traverse.Create(__instance).Property("CurrentDesiredMoodBreakIntensity")
                    .GetValue<MentalBreakIntensity>(), out var intensity);
            // Log.Message("Mental break had an intensity of " + intensity);
            var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (pawn.kindDef.race.defName != "Ant_AntiniumRace" || !__result || intensity < 2)
            {
                return;
            }

            // Log.Message("it might be an aberration?");
            if (!Rand.Chance((intensity * .06f) - .1f))
            {
                return;
            }

            pawn.health.AddHediff(AntDefOf.Ant_Aberration);
            Find.LetterStack.ReceiveLetter("LetterLabelAberration".Translate(pawn),
                "LetterAberration".Translate(pawn), LetterDefOf.NegativeEvent);
        }


        // drug resistance
        public static void ModifyChemicalEffectForToleranceAndBodySize_Postfix(Pawn pawn, ChemicalDef chemicalDef,
            ref float effect)
        {
            if (chemicalDef == null)
            {
                return;
            }

            if (pawn.kindDef.race.defName == "Ant_AntiniumRace" && chemicalDef.defName != "Luciferium")
            {
                effect *= .6f;
            }
        }

        #endregion
    }
}