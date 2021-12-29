using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using rjw;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using RimWorld.Planet;


namespace SlimeGirl
{

    [StaticConstructorOnStartup]
    public class SlimeCorpse : Corpse
    {
        public static readonly string corpsPath = "Things/Slime_Corpse";

        public static readonly Graphic slimeCorpseTexture = GraphicDatabase.Get<Graphic_Single>(path: corpsPath, shader: ShaderDatabase.Cutout, drawSize: new Vector2(2, 2), color: Color.white);

        public int innerTime;
        public int reviveTime;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            reviveTime = GenTicks.SecondsToTicks(1000);
            innerTime = reviveTime;
            IncidentWorker_SlimeTransportPodCrash.livingSlime = null;
            IncidentWorker_SlimeTransportPodCrash.slimeCorps = this;
        }

        public override void TickRare()
        {
            base.TickRare();
            if (Destroyed)
            {
                return;
            }
            if (Bugged)
            {
                Log.Error(this + " has null innerPawn. Destroying.", false);
                Destroy(DestroyMode.Vanish);
                return;
            }
            if (innerTime > 0)
            {
                innerTime += -250;
            }
            else
            {
                Resurrect();
                innerTime = reviveTime;
            }

        }

        private void Resurrect()
        {
            Pawn innerPawn = InnerPawn;
            ResurrectionUtility.Resurrect(innerPawn);
            Messages.Message("MessagePawnResurrected".Translate(innerPawn).CapitalizeFirst(), innerPawn, MessageTypeDefOf.PositiveEvent, true);
            IncidentWorker_SlimeTransportPodCrash.livingSlime = InnerPawn;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref innerTime, "innerTime");
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            IncidentWorker_SlimeTransportPodCrash.slimeCorps = null;
            base.Destroy(mode);
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (InnerPawn.Faction != null)
            {
                stringBuilder.AppendLine("Faction".Translate() + ": " + InnerPawn.Faction.Name);
            }
            stringBuilder.AppendLine("DeadTime".Translate(Age.ToStringTicksToPeriodVague(true, false)));
            float num = 1f - InnerPawn.health.hediffSet.GetCoverageOfNotMissingNaturalParts(InnerPawn.RaceProps.body.corePart);
            if (num != 0f)
            {
                stringBuilder.AppendLine("RemainTime".Translate() + ": " + innerTime.ToStringTicksToPeriod());
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            slimeCorpseTexture.Draw(drawLoc, Rot4.North, this, 0f);
        }
    }

}
