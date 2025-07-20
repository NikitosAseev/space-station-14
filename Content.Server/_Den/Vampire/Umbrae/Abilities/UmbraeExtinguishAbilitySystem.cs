using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared._Den.Vampire.Umbrae.Abilities;
using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared._Den.Vampire.Umbrae.Events;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Server.GameObjects;

namespace Content.Server._Den.Vampire.Umbrae.Abilities;

public sealed class UmbraeExtinguishAbilitySystem : SharedUmbraeExtinguishAbilitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpoweredFlashlight = default!;
    [Dependency] private readonly SharedHandheldLightSystem  _handheldLight = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLightSystem =  default!;

    public override void OnExtinguish(Entity<UmbraeExtinguishAbilityComponent> ent, ref UmbraeExtinguishAbilityEvent args)
    {
        if (args.Handled)
            return;

        foreach (var lightEnt in _lookup.GetEntitiesInRange<PointLightComponent>(_transform.GetMapCoordinates(ent), ent.Comp.Range))
        {
                if (TryComp<PoweredLightComponent>(lightEnt, out var poweredLightComp))
                    _poweredLightSystem.SetState(lightEnt, false, poweredLightComp);

                if (TryComp<HandheldLightComponent>(lightEnt, out var handheldLightComp))
                     _handheldLight.TurnOff((lightEnt, handheldLightComp));

                if (TryComp<UnpoweredFlashlightComponent>(lightEnt, out var unpoweredFlashlightComp))
                    _unpoweredFlashlight.SetLight((lightEnt, unpoweredFlashlightComp), false);
        }

        args.Handled = true;
    }
}
