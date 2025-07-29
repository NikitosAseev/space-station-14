using System.Numerics;
using Content.Server._Den.Light;
using Content.Shared._Den.Vampire.Umbrae.Abilities;
using Content.Shared._Den.Vampire.Umbrae.Abilities.Components;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Shared._Den.Light;


namespace Content.Server._Den.Vampire.Umbrae.Abilities;

public sealed class UmbraeCloakOfDarknessAbilitySystem : SharedUmbraeCloakOfDarknessAbilitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly PointLightSystem _lightSystem = default!;
    [Dependency] private readonly LightIntensitySystem _lightIntensity = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UmbraeCloakOfDarknessAbilityComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextCloakOfDarknessUpdate = _timing.CurTime;
    }

    public void CalculateLocalLight(Entity<UmbraeCloakOfDarknessAbilityComponent> ent, StealthComponent stealth,  TransformComponent xform)
    {
        if (!TryComp<LightDetectionComponent>(ent.Owner, out var lightDetect))
            return;

        var light = lightDetect.CurrentIntensity;

        var bestVisibility = Math.Clamp(light, stealth.MinVisibility, stealth.MaxVisibility);

        Logger.Info($"bestVisibility : {bestVisibility}");

        _stealth.SetVisibility(ent, bestVisibility, stealth);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<UmbraeCloakOfDarknessAbilityComponent, StealthComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var comp, out var stealth, out var xform))
        {
            if(!stealth.Enabled)
                continue;

            if (_timing.CurTime >= comp.NextCloakOfDarknessUpdate)
            {
                comp.NextCloakOfDarknessUpdate = _timing.CurTime + comp.UpdateCloakOfDarknessInterval;
                CalculateLocalLight((ent, comp), stealth, xform);
            }
        }
    }
}
