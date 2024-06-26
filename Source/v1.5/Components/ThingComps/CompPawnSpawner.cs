﻿using Verse;
using RimWorld;

namespace ArtificialBeings
{
    public class CompPawnSpawner : ThingComp
    {
        public CompProperties_PawnSpawner Props
        {
            get
            {
                return props as CompProperties_PawnSpawner;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            SpawnPawn();
            parent.Destroy();
        }

        // Fail-safe if an error should happen to occur at spawn, attempt to do it again repeatedly somewhat infrequently until success.
        public override void CompTick()
        {
            if (parent.IsHashIntervalTick(339))
            {
                SpawnPawn();
                parent.Destroy();
            }
        }

        // Generate and spawn the created pawn.
        public virtual Pawn SpawnPawn()
        {
            PawnGenerationRequest request = new PawnGenerationRequest(Props.pawnKind, Faction.OfPlayer, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowFood: false, allowAddictions: false, fixedBiologicalAge: 0, fixedChronologicalAge: 0, fixedIdeo: null, forceNoIdeo: true, forceBaselinerChance: 1f);
            Pawn pawn = PawnGenerator.GeneratePawn(request);

            // Pawns may sometimes spawn with apparel somewhere in the generation process. Ensure they don't actually spawn with any - if they even can have apparel.
            pawn.apparel?.DestroyAll();

            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
            Find.LetterStack.ReceiveLetter("ABF_NewbootSynstructCreatedLabel".Translate(), "ABF_NewbootSynstructCreatedText".Translate(pawn.def.label), LetterDefOf.PositiveEvent, pawn, hyperlinkThingDefs: new System.Collections.Generic.List<ThingDef> { pawn.def });

            return pawn;
        }
    }
}