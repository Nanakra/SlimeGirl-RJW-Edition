using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;
using RimWorld.Planet;



namespace SlimeGirl
{

    public class IncidentWorker_SlimeTransportPodCrash : IncidentWorker
    {
        public static Thing livingSlime;
        public static Thing slimeCorps;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            ThingRequest slimeReq = new ThingRequest();
            slimeReq.singleDef = SlimeRaceDefOf.Slime;
            ThingRequest slimeCorpseReq = new ThingRequest();
            slimeCorpseReq.singleDef = SlimeRaceDefOf.Corpse_Slime;

            foreach (Map m in Find.Maps)
            {
                if(m.listerThings.ThingsMatching(slimeCorpseReq).Count>0 || m.listerThings.ThingsMatching(slimeReq).Count>0)
                {
                    return false;
                }
            }
            return true;
        }
        

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Thing> things = new List<Thing>();
            PawnGenerationRequest request = new PawnGenerationRequest(
                kind: SlimePawnKindDefOf.Slime,
                faction: Find.FactionManager.FirstFactionOfDef(SlimeFactionDefOf.SpaceSubjects),
                colonistRelationChanceFactor: 0,
                allowGay: false
                );
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            pawn.health.AddHediff(RimWorld.HediffDefOf.Anesthetic);
            things.Add(pawn);
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);

            TaggedString label = "SlimeLetterLabelRefugeePodCrash".Translate();
            TaggedString text = "SlimeRefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            text += "\n\n";
            if (pawn.Faction == null)
            {
                text += "SlimeRefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            }
            else if (pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                text += "SlimeRefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            }
            else
            {
                text += "SlimeRefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN");
            }
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map, false), null, null);
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things, true, false);
            activeDropPodInfo.openDelay = 180;
            activeDropPodInfo.leaveSlag = true;
            DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);
            return true;
        }

        private Pawn FindPawn(List<Thing> things)
        {
            for (int i = 0; i < things.Count; i++)
            {
                Pawn pawn = things[i] as Pawn;
                if (pawn != null)
                {
                    return pawn;
                }
                Corpse corpse = things[i] as Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
            }
            return null;
        }
    }
}
