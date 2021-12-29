using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using rjw;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
namespace SlimeGirl
{
    public class HediffCompProperties_AutoRecovery : HediffCompProperties
    {
        public int tickMultiflier = 6000;
        public float healPoint = 1;
        public HediffCompProperties_AutoRecovery()
        {
            compClass = typeof(HediffComp_AutoRecovery);
        }
    }

    public class HediffComp_AutoRecovery : HediffComp
    {
        public static Dictionary<BodyPartDef, HediffDef> bodyMaps = new Dictionary<BodyPartDef, HediffDef> { { xxx.anusDef, Genital_Helper.slime_anus }, { xxx.genitalsDef, Genital_Helper.slime_vagina }, { xxx.breastsDef, Genital_Helper.slime_breasts } };
        public static int count=0;
        private int ticksToHeal;
        private float healPoint;
        public static Func<Hediff, bool> func;
        public HediffCompProperties_AutoRecovery Props
        {
            get
            {
                return (HediffCompProperties_AutoRecovery)props;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            ResetTicksToHeal();
            if (Pawn.health.hediffSet.hediffs.Find((Hediff h) => h.def == Genital_Helper.slime_breasts) == null)
            {
                Pawn.health.AddHediff(Genital_Helper.slime_breasts, Pawn.RaceProps.body.AllParts.Find((BodyPartRecord x) => x.def.defName == "Chest"));
            }
            if (Pawn.health.hediffSet.hediffs.Find((Hediff h) => h.def == Genital_Helper.slime_penis) ==null)
            {
                return;
            }
            Pawn.health.RemoveHediff(Pawn.health.hediffSet.hediffs.Find((Hediff h) => h.def == Genital_Helper.slime_penis));

        }

        private void ResetTicksToHeal()
        {
            healPoint = Rand.Range(1, 3) * Props.healPoint;
            ticksToHeal = Rand.Range(15, 30) * Props.tickMultiflier;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            ticksToHeal--;
            if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                ResetTicksToHeal();
            }
        }

        private void TryHealRandomPermanentWound()
        {
            List<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
            for (int i =0; i<hediffs.Count;i++)
            {
                if (hediffs[i] is Hediff_Injury)
                {
                    hediffs[i].Severity -= healPoint;
                }
                if (hediffs[i] is Hediff_MissingPart)
                {
                    BodyPartRecord bodyPartRecord= hediffs[i].Part;
                    Pawn.health.RestorePart(hediffs[i].Part);
                    if(bodyMaps.ContainsKey(bodyPartRecord.def))
                    {
                        //Log.Message(bodyPartRecord.def.defName);
                        Pawn.health.AddHediff(bodyMaps[bodyPartRecord.def], bodyPartRecord);
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal", 0, false);
        }

        public override string CompDebugString()
        {
            return "ticksToHeal: " + ticksToHeal;
        }

    }
}
