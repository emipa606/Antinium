using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;


namespace AntiniumHiveQueen
{
    public class Hediff_RoyalLarva : HediffWithComps 
    {
        public override void Tick ()
        {
            long ageIncr = 0;
            switch(CurStageIndex)
            {
                case 0:
                    ageIncr = 6;
                    break;
                case 1:
                    ageIncr = 12;
                    break;
                case 2:
                    ageIncr = 18;
                    break;
                case 3:
                    ageIncr = 24;
                    break;


                //case 0:
                //    ageIncr = 15;
                //    break;
                //case 1:
                //    ageIncr = 25;
                //    break;
                //case 2:
                //    ageIncr = 40;
                //    break;
                //case 3:
                //    ageIncr = 60;
                //    break;

                default:
                    break;
            }


            pawn.ageTracker.AgeBiologicalTicks += ageIncr;

            //commented out to see where it ends naturallys

            //if (pawn.ageTracker.AgeBiologicalYears >= 20)
            //{
            //    // TODO: Add adult backstory!

            //    this.Severity = 0;
            //}
        }
    }
}
