using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared._Den.Vampire.Umbrae.Events;
using Content.Shared._Den.Vampire.Umbrae.Systems;
using Content.Shared.Actions;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Den.Vampire.Umbrae.Abilities;

public sealed class UmbraeSoulAnchorAbilitySystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UmbraeSoulAnchorAbilityComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<UmbraeSoulAnchorAbilityComponent, UmbraeSoulAnchorAbilityEvent>(OnSoulAnchor);
        SubscribeLocalEvent<UmbraeSoulAnchorComponent, TimedDespawnEvent>(OnSoulAnchorDespawned);
    }

    private void OnMapInit(Entity<UmbraeSoulAnchorAbilityComponent> ent, ref MapInitEvent args)
    {
        _action.AddAction(ent, ref ent.Comp.Action, ent.Comp.ActionProto, ent);
    }

  private void OnSoulAnchor(Entity<UmbraeSoulAnchorAbilityComponent> ent, ref UmbraeSoulAnchorAbilityEvent args)
    {
         if (args.Handled || !_timing.IsFirstTimePredicted)
             return;

         if (ent.Comp.ActiveAnchor.HasValue && !_entityManager.EntityExists(ent.Comp.ActiveAnchor.Value))
             ent.Comp.ActiveAnchor = null;

         if (ent.Comp.ActiveAnchor.HasValue && _entityManager.EntityExists(ent.Comp.ActiveAnchor.Value))
         {
             TryTeleportAndCleanup(ent, ent.Comp.ActiveAnchor.Value);
             args.Handled = true;
             return;
         }

         SpawnAnchorAndStartTimer(ent);
    }
    private void SpawnAnchorAndStartTimer(Entity<UmbraeSoulAnchorAbilityComponent> ent)
    {
        var anchor = PredictedSpawnAtPosition(ent.Comp.SoulAnchor, Transform(ent).Coordinates);

        ent.Comp.ActiveAnchor = anchor;

        var anchorComp = EnsureComp<UmbraeSoulAnchorComponent>(anchor);
        anchorComp.OwnerSoulAnchor = ent;
        var despawn = EnsureComp<TimedDespawnComponent>(anchor);
        despawn.Lifetime = ent.Comp.TeleportTimer;


    }
    private void TryTeleportAndCleanup(Entity<UmbraeSoulAnchorAbilityComponent> ent, EntityUid anchor)
    {
        _transform.SetCoordinates(ent, Transform(anchor).Coordinates);
        _entityManager.PredictedDeleteEntity(anchor);
        ent.Comp.ActiveAnchor = null;
    }

    private void OnSoulAnchorDespawned(Entity<UmbraeSoulAnchorComponent> soulAnchor, ref TimedDespawnEvent args)
    {
        if (soulAnchor.Comp.OwnerSoulAnchor.HasValue)
        {
            var ownerSoulAnchor = soulAnchor.Comp.OwnerSoulAnchor.Value;
           if (TryComp<UmbraeSoulAnchorAbilityComponent>(soulAnchor.Comp.OwnerSoulAnchor, out var anchor))
               TryTeleportAndCleanup((ownerSoulAnchor, anchor), soulAnchor);
        }
    }
}

