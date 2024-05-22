﻿using Verse;
using HarmonyLib;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace ArtificialBeings
{
    public class Pawn_JobTracker_Patch
    {
        // Charge-capable pawns that are tucked in (via capture, rescue, for operations, etc) to a charge-capable bed will charge instead of resting normally.
        [HarmonyPatch(typeof(Pawn_JobTracker), "Notify_TuckedIntoBed")]
        public static class Notify_TuckedIntoBed_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(Pawn_JobTracker __instance, Pawn ___pawn, Building_Bed bed)
            {
                if (SC_Utils.CanCharge(___pawn))
                {
                    bool isChargeCapable = false;

                    // Charging bed check (the building itself has a CompPawnCharger)
                    if (bed != null && bed.GetComp<CompPawnCharger>() != null && bed.GetComp<CompPowerTrader>()?.PowerOn == true)
                    {
                        isChargeCapable = true;
                    }

                    // Normal bed with an attached bedside charger check (A linked building has a CompPawnCharger)
                    List<Thing> linkedBuildings = bed.GetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading;
                    if (linkedBuildings != null)
                    {
                        foreach (Thing linkedBuilding in linkedBuildings)
                        {
                            if (linkedBuilding.TryGetComp<CompPawnCharger>() != null && linkedBuilding.TryGetComp<CompPowerTrader>()?.PowerOn == true)
                            {
                                isChargeCapable = true;
                            }
                        }
                    }

                    if (isChargeCapable)
                    {
                        ___pawn.Position = RestUtility.GetBedSleepingSlotPosFor(___pawn, bed);
                        ___pawn.Notify_Teleported(endCurrentJob: false);
                        ___pawn.stances.CancelBusyStanceHard();
                        __instance.StartJob(JobMaker.MakeJob(ABF_JobDefOf.ABF_GetRecharge, bed), JobCondition.InterruptForced, tag: JobTag.TuckedIntoBed);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}